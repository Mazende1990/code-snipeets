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
        public async Task<ActionResult<BaseItemDto>> GetItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            await RefreshItemOnDemandIfNeeded(item);
            
            return _dtoService.GetBaseItemDto(item, CreateDtoOptions(), user);
        }

        [HttpGet("Users/{userId}/Items/Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<BaseItemDto> GetRootFolder([FromRoute, Required] Guid userId)
        {
            var user = _userManager.GetUserById(userId);
            var item = _libraryManager.GetUserRootFolder();
            return _dtoService.GetBaseItemDto(item, CreateDtoOptions(), user);
        }

        [HttpGet("Users/{userId}/Items/{itemId}/Intros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetIntros(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var intros = await _libraryManager.GetIntros(item, user);
            
            var dtos = intros
                .Select(i => _dtoService.GetBaseItemDto(i, CreateDtoOptions(), user))
                .ToArray();

            return new QueryResult<BaseItemDto>
            {
                Items = dtos,
                TotalRecordCount = dtos.Length
            };
        }

        [HttpPost("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> MarkFavoriteItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateFavoriteStatus(userId, itemId, true);
        }

        [HttpDelete("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UnmarkFavoriteItem(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateFavoriteStatus(userId, itemId, false);
        }

        [HttpDelete("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> DeleteUserItemRating(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            return UpdateUserItemRating(userId, itemId, null);
        }

        [HttpPost("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UpdateUserItemRating(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId, 
            [FromQuery] bool? likes)
        {
            return UpdateUserItemRating(userId, itemId, likes);
        }

        [HttpGet("Users/{userId}/Items/{itemId}/LocalTrailers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetLocalTrailers(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var dtoOptions = CreateDtoOptions();

            var trailers = item.GetExtras(new[] { ExtraType.Trailer })
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item))
                .ToList();

            if (item is IHasTrailers hasTrailers)
            {
                trailers.AddRange(_dtoService.GetBaseItemDtos(hasTrailers.LocalTrailers, dtoOptions, user, item));
            }

            return trailers;
        }

        [HttpGet("Users/{userId}/Items/{itemId}/SpecialFeatures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetSpecialFeatures(
            [FromRoute, Required] Guid userId, 
            [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var dtoOptions = CreateDtoOptions();

            return item.GetExtras(BaseItem.DisplayExtraTypes)
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item));
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
            isPlayed = DetermineIsPlayedFilter(user, isPlayed);

            var latestItems = _userViewManager.GetLatestItems(
                new LatestItemsQuery
                {
                    GroupItems = groupItems,
                    IncludeItemTypes = includeItemTypes,
                    IsPlayed = isPlayed,
                    Limit = limit,
                    ParentId = parentId ?? Guid.Empty,
                    UserId = userId,
                },
                CreateDtoOptions(fields, enableImages, enableUserData, imageTypeLimit, enableImageTypes));

            return latestItems.Select(i => CreateLatestItemDto(i, user));
        }

        private BaseItem GetItemById(Guid itemId)
        {
            return itemId.Equals(Guid.Empty) 
                ? _libraryManager.GetUserRootFolder() 
                : _libraryManager.GetItemById(itemId);
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

        private async Task RefreshItemOnDemandIfNeeded(BaseItem item)
        {
            if (item is Person person && NeedsMetadataRefresh(person))
            {
                var options = new MetadataRefreshOptions(new DirectoryService(_fileSystem))
                {
                    MetadataRefreshMode = MetadataRefreshMode.FullRefresh,
                    ImageRefreshMode = MetadataRefreshMode.FullRefresh,
                    ForceSave = NeedsFullRefresh(person)
                };

                await person.RefreshMetadata(options, CancellationToken.None);
            }
        }

        private bool NeedsMetadataRefresh(Person person)
        {
            return string.IsNullOrWhiteSpace(person.Overview) || !person.HasImage(ImageType.Primary);
        }

        private bool NeedsFullRefresh(Person person)
        {
            return NeedsMetadataRefresh(person) && (DateTime.UtcNow - person.DateLastRefreshed).TotalDays >= 3;
        }

        private UserItemDataDto UpdateFavoriteStatus(Guid userId, Guid itemId, bool isFavorite)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var userData = _userDataRepository.GetUserData(user, item);

            userData.IsFavorite = isFavorite;
            SaveUserData(user, item, userData);

            return _userDataRepository.GetUserDataDto(item, user);
        }

        private UserItemDataDto UpdateUserItemRating(Guid userId, Guid itemId, bool? likes)
        {
            var user = _userManager.GetUserById(userId);
            var item = GetItemById(itemId);
            var userData = _userDataRepository.GetUserData(user, item);

            userData.Likes = likes;
            SaveUserData(user, item, userData);

            return _userDataRepository.GetUserDataDto(item, user);
        }

        private void SaveUserData(User user, BaseItem item, UserItemData userData)
        {
            _userDataRepository.SaveUserData(
                user, 
                item, 
                userData, 
                UserDataSaveReason.UpdateUserRating, 
                CancellationToken.None);
        }

        private bool? DetermineIsPlayedFilter(User user, bool? isPlayed)
        {
            return isPlayed ?? (user.HidePlayedInLatest ? false : (bool?)null);
        }

        private BaseItemDto CreateLatestItemDto((BaseItem, List<BaseItem>) itemGroup, User user)
        {
            var (parent, items) = itemGroup;
            var dtoOptions = CreateDtoOptions();
            
            var item = items.Count > 1 || parent is MusicAlbum ? parent : items[0];
            var dto = _dtoService.GetBaseItemDto(item, dtoOptions, user);
            
            if (parent != null && (items.Count > 1 || parent is MusicAlbum))
            {
                dto.ChildCount = items.Count;
            }

            return dto;
        }
    }
}