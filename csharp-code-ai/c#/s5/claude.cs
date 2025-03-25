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
    /// <summary>
    /// Manages user library operations such as retrieving items, marking favorites, and updating ratings.
    /// </summary>
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

        /// <summary>
        /// Retrieves a specific item from a user's library.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseItemDto>> GetItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);

            await RefreshPersonMetadataIfNeeded(item).ConfigureAwait(false);

            var dtoOptions = CreateDtoOptions();
            return _dtoService.GetBaseItemDto(item, dtoOptions, user);
        }

        /// <summary>
        /// Retrieves the root folder of a user's library.
        /// </summary>
        [HttpGet("Users/{userId}/Items/Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<BaseItemDto> GetRootFolder([FromRoute, Required] Guid userId)
        {
            var user = _userManager.GetUserById(userId);
            var item = _libraryManager.GetUserRootFolder();
            var dtoOptions = CreateDtoOptions();
            return _dtoService.GetBaseItemDto(item, dtoOptions, user);
        }

        /// <summary>
        /// Retrieves intros for a specific media item.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/Intros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetIntros(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);

            var items = await _libraryManager.GetIntros(item, user).ConfigureAwait(false);
            var dtoOptions = CreateDtoOptions();
            var dtos = items.Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user)).ToArray();

            return new QueryResult<BaseItemDto>
            {
                Items = dtos,
                TotalRecordCount = dtos.Length
            };
        }

        /// <summary>
        /// Marks an item as a favorite.
        /// </summary>
        [HttpPost("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> MarkFavoriteItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateFavoriteStatus(userId, itemId, true);
        }

        /// <summary>
        /// Removes an item from favorites.
        /// </summary>
        [HttpDelete("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UnmarkFavoriteItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateFavoriteStatus(userId, itemId, false);
        }

        /// <summary>
        /// Deletes a user's personal rating for an item.
        /// </summary>
        [HttpDelete("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> DeleteUserItemRating(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateUserItemRating(userId, itemId, null);
        }

        /// <summary>
        /// Updates a user's rating for an item.
        /// </summary>
        [HttpPost("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UpdateUserItemRating(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId, 
            [FromQuery] bool? likes)
        {
            return UpdateUserItemRatingInternal(userId, itemId, likes);
        }

        /// <summary>
        /// Retrieves local trailers for an item.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/LocalTrailers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetLocalTrailers(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var dtoOptions = CreateDtoOptions();

            var dtosExtras = item.GetExtras(new[] { ExtraType.Trailer })
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item))
                .ToArray();

            if (item is IHasTrailers hasTrailers)
            {
                var trailers = hasTrailers.LocalTrailers;
                var dtosTrailers = _dtoService.GetBaseItemDtos(trailers, dtoOptions, user, item);
                
                return CombineTrailers(dtosExtras, dtosTrailers);
            }

            return dtosExtras;
        }

        /// <summary>
        /// Retrieves special features for an item.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/SpecialFeatures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetSpecialFeatures(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var dtoOptions = CreateDtoOptions();

            return Ok(item
                .GetExtras(BaseItem.DisplayExtraTypes)
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item)));
        }

        /// <summary>
        /// Retrieves the latest media for a user.
        /// </summary>
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
            isPlayed = DeterminePlayedFilter(user, isPlayed);

            var dtoOptions = CreateDtoOptions(fields, enableImages, enableUserData, imageTypeLimit, enableImageTypes);

            var list = _userViewManager.GetLatestItems(
                new LatestItemsQuery
                {
                    GroupItems = groupItems,
                    IncludeItemTypes = includeItemTypes,
                    IsPlayed = isPlayed,
                    Limit = limit,
                    ParentId = parentId ?? Guid.Empty,
                    UserId = userId,
                },
                dtoOptions);

            return Ok(MapLatestItems(list, dtoOptions, user));
        }

        private BaseItem GetItemById(Guid itemId)
        {
            return itemId.Equals(Guid.Empty) 
                ? _libraryManager.GetUserRootFolder() 
                : _libraryManager.GetItemById(itemId);
        }

        private async Task RefreshPersonMetadataIfNeeded(BaseItem item)
        {
            if (item is Person person)
            {
                var hasMetadata = !string.IsNullOrWhiteSpace(person.Overview) && person.HasImage(ImageType.Primary);
                var performFullRefresh = !hasMetadata && (DateTime.UtcNow - item.DateLastRefreshed).TotalDays >= 3;

                if (!hasMetadata)
                {
                    var options = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
                    {
                        MetadataRefreshMode = MetadataRefreshMode.FullRefresh,
                        ImageRefreshMode = MetadataRefreshMode.FullRefresh,
                        ForceSave = performFullRefresh
                    };

                    await item.RefreshMetadata(options, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        private DtoOptions CreateDtoOptions(
            ItemFields[] fields = null, 
            bool? enableImages = null, 
            bool? enableUserData = null, 
            int? imageTypeLimit = null, 
            ImageType[] enableImageTypes = null)
        {
            return new DtoOptions { Fields = fields }
                .AddClientFields(Request)
                .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);
        }

        private bool? DeterminePlayedFilter(User user, bool? isPlayed)
        {
            if (!isPlayed.HasValue && user.HidePlayedInLatest)
            {
                return false;
            }
            return isPlayed;
        }

        private IEnumerable<BaseItemDto> MapLatestItems(
            IEnumerable<(BaseItem, List<BaseItem>)> list, 
            DtoOptions dtoOptions, 
            User user)
        {
            return list.Select(i =>
            {
                var item = i.Item2[0];
                var childCount = 0;

                if (i.Item1 != null && (i.Item2.Count > 1 || i.Item1 is MusicAlbum))
                {
                    item = i.Item1;
                    childCount = i.Item2.Count;
                }

                var dto = _dtoService.GetBaseItemDto(item, dtoOptions, user);
                dto.ChildCount = childCount;

                return dto;
            });
        }

        private UserItemDataDto UpdateFavoriteStatus(Guid userId, Guid itemId, bool isFavorite)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);

            var data = _userDataRepository.GetUserData(user, item);
            data.IsFavorite = isFavorite;

            _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);

            return _userDataRepository.GetUserDataDto(item, user);
        }

        private UserItemDataDto UpdateUserItemRatingInternal(Guid userId, Guid itemId, bool? likes)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);

            var data = _userDataRepository.GetUserData(user, item);
            data.Likes = likes;

            _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);

            return _userDataRepository.GetUserDataDto(item, user);
        }

        private BaseItemDto[] CombineTrailers(BaseItemDto[] extras, IList<BaseItemDto> trailers)
        {
            var allTrailers = new BaseItemDto[extras.Length + trailers.Count];
            extras.CopyTo(allTrailers, 0);
            trailers.CopyTo(allTrailers, extras.Length);
            return allTrailers;
        }
    }
}