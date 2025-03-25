using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Api.Constants;
using Jellyfin.Api.Extensions;
using Jellyfin.Api.ModelBinders;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers
{
    [Route("")]
    [Authorize(Policy = Policies.DefaultAuthorization)]
    public class UserLibraryController : BaseJellyfinApiController
    {
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataRepository;
        private readonly ILibraryManager _libraryManager;
        private readonly IDtoService _dtoService;
        private readonly IUserViewManager _userViewManager;
        private readonly IFileSystem _fileSystem;

        public UserLibraryController(
            IUserManager userManager,
            IUserDataManager userDataRepository,
            ILibraryManager libraryManager,
            IDtoService dtoService,
            IUserViewManager userViewManager,
            IFileSystem fileSystem)
        {
            _userManager = userManager;
            _userDataRepository = userDataRepository;
            _libraryManager = libraryManager;
            _dtoService = dtoService;
            _userViewManager = userViewManager;
            _fileSystem = fileSystem;
        }

        [HttpGet("Users/{userId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseItemDto>> GetItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            await RefreshItemOnDemandIfNeeded(item);
            var dtoOptions = new DtoOptions().AddClientFields(Request);
            return _dtoService.GetBaseItemDto(item, dtoOptions, user);
        }

        [HttpGet("Users/{userId}/Items/Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<BaseItemDto> GetRootFolder([FromRoute, Required] Guid userId)
        {
            var user = _userManager.GetUserById(userId);
            var dtoOptions = new DtoOptions().AddClientFields(Request);
            return _dtoService.GetBaseItemDto(_libraryManager.GetUserRootFolder(), dtoOptions, user);
        }

        [HttpGet("Users/{userId}/Items/{itemId}/Intros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetIntros([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            var intros = await _libraryManager.GetIntros(item, user);
            var dtoOptions = new DtoOptions().AddClientFields(Request);
            var dtos = intros.Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user)).ToArray();

            return new QueryResult<BaseItemDto> { Items = dtos, TotalRecordCount = dtos.Length };
        }

        [HttpPost("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> MarkFavoriteItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
            => MarkFavorite(userId, itemId, true);

        [HttpDelete("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UnmarkFavoriteItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
            => MarkFavorite(userId, itemId, false);

        [HttpDelete("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> DeleteUserItemRating([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
            => UpdateUserItemRatingInternal(userId, itemId, null);

        [HttpPost("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UpdateUserItemRating([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId, [FromQuery] bool? likes)
            => UpdateUserItemRatingInternal(userId, itemId, likes);

        [HttpGet("Users/{userId}/Items/{itemId}/LocalTrailers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetLocalTrailers([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            var dtoOptions = new DtoOptions().AddClientFields(Request);

            var extras = item.GetExtras(new[] { ExtraType.Trailer })
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item)).ToArray();

            if (item is IHasTrailers hasTrailers)
            {
                var trailers = _dtoService.GetBaseItemDtos(hasTrailers.LocalTrailers, dtoOptions, user, item);
                return extras.Concat(trailers).ToArray();
            }

            return extras;
        }

        [HttpGet("Users/{userId}/Items/{itemId}/SpecialFeatures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetSpecialFeatures([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            var dtoOptions = new DtoOptions().AddClientFields(Request);

            return Ok(item.GetExtras(BaseItem.DisplayExtraTypes)
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item)));
        }

        [HttpGet("Users/{userId}/Items/Latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetLatestMedia(
            [FromRoute, Required] Guid userId,
            [FromQuery] Guid? parentId,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ItemFields[] fields,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] BaseItemKind[] includeItemTypes,
            [FromQuery] bool? isPlayed,
            [FromQuery] bool? enableImages,
            [FromQuery] int? imageTypeLimit,
            [FromQuery, ModelBinder(typeof(CommaDelimitedArrayModelBinder))] ImageType[] enableImageTypes,
            [FromQuery] bool? enableUserData,
            [FromQuery] int limit = 20,
            [FromQuery] bool groupItems = true)
        {
            var user = _userManager.GetUserById(userId);
            isPlayed ??= user.HidePlayedInLatest ? false : null;

            var dtoOptions = new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);

            var items = _userViewManager.GetLatestItems(new LatestItemsQuery
            {
                GroupItems = groupItems,
                IncludeItemTypes = includeItemTypes,
                IsPlayed = isPlayed,
                Limit = limit,
                ParentId = parentId ?? Guid.Empty,
                UserId = userId,
            }, dtoOptions);

            return Ok(items.Select(i =>
            {
                var item = i.Item2.First();
                var childCount = 0;

                if (i.Item1 != null && (i.Item2.Count > 1 || i.Item1 is MusicAlbum))
                {
                    item = i.Item1;
                    childCount = i.Item2.Count;
                }

                var dto = _dtoService.GetBaseItemDto(item, dtoOptions, user);
                dto.ChildCount = childCount;
                return dto;
            }));
        }

        private BaseItem GetItemOrRoot(Guid itemId)
            => itemId == Guid.Empty ? _libraryManager.GetUserRootFolder() : _libraryManager.GetItemById(itemId);

        private async Task RefreshItemOnDemandIfNeeded(BaseItem item)
        {
            if (item is Person person)
            {
                var hasMetadata = !string.IsNullOrWhiteSpace(person.Overview) && person.HasImage(ImageType.Primary);
                var refreshNeeded = !hasMetadata && (DateTime.UtcNow - person.DateLastRefreshed).TotalDays >= 3;

                if (!hasMetadata)
                {
                    var options = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
                    {
                        MetadataRefreshMode = MetadataRefreshMode.FullRefresh,
                        ImageRefreshMode = MetadataRefreshMode.FullRefresh,
                        ForceSave = refreshNeeded
                    };

                    await person.RefreshMetadata(options, CancellationToken.None);
                }
            }
        }

        private UserItemDataDto MarkFavorite(Guid userId, Guid itemId, bool isFavorite)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            var data = _userDataRepository.GetUserData(user, item);
            data.IsFavorite = isFavorite;
            _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);
            return _userDataRepository.GetUserDataDto(item, user);
        }

        private UserItemDataDto UpdateUserItemRatingInternal(Guid userId, Guid itemId, bool? likes)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemOrRoot(itemId);
            var data = _userDataRepository.GetUserData(user, item);
            data.Likes = likes;
            _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);
            return _userDataRepository.GetUserDataDto(item, user);
        }
    }
}
