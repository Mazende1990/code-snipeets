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
    /// User library controller.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLibraryController"/> class.
        /// </summary>
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
            = libraryManager;
            _dtoService = dtoService;
            _userViewManager = userViewManager;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Gets an item from a user's library.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<BaseItemDto>> GetItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = itemId.Equals(Guid.Empty)
                ? _libraryManager.GetUserRootFolder()
                : _libraryManager.GetItemById(itemId);

            await RefreshItemOnDemandIfNeeded(item).ConfigureAwait(false);

            var dtoOptions = new DtoOptions().AddClientFields(Request);
            return _dtoService.GetBaseItemDto(item, dtoOptions, user);
        }

        /// <summary>
        /// Gets the root folder from a user's library.
        /// </summary>
        [HttpGet("Users/{userId}/Items/Root")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<BaseItemDto> GetRootFolder([FromRoute, Required] Guid userId)
        {
            var user = _userManager.GetUserById(userId);
            var item = _libraryManager.GetUserRootFolder();
            var dtoOptions = new DtoOptions().AddClientFields(Request);
            return _dtoService.GetBaseItemDto(item, dtoOptions, user);
        }

        /// <summary>
        /// Gets intros to play before the main media item plays.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/Intros")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<QueryResult<BaseItemDto>>> GetIntros([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = itemId.Equals(Guid.Empty)
                _libraryManager.GetUserRootFolder()
                : _libraryManager.GetItemById(itemId);

            var items = await _libraryManager.GetIntros(item, user).ConfigureAwait(false);
            var dtoOptions = new DtoOptions().AddClientFields(Request);
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
        public ActionResult<UserItemDataDto> MarkFavoriteItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            return MarkFavorite(userId, itemId, true);
        }

        /// <summary>
        /// Unmarks item as a favorite.
        /// </summary>
        [HttpDelete("Users/{userId}/FavoriteItems/{itemId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UnmarkFavoriteItem([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            return MarkFavorite(userId, itemId, false);
        }

        /// <summary>
        /// Deletes a user's saved personal rating for an item.
        /// </summary>
        [HttpDelete("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> DeleteUserItemRating([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            return UpdateUserItemRatingInternal(userId, itemId, null);
        }

        /// <summary>
        /// Updates a user's rating for an item.
        /// </summary>
        [HttpPost("Users/{userId}/Items/{itemId}/Rating")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserItemDataDto> UpdateUserItemRating([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId, [FromQuery] bool? likes)
        {
            return UpdateUserItemRatingInternal(userId, itemId, likes);
        }

        /// <summary>
        /// Gets local trailers for an item.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/LocalTrailers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetLocalTrailers([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        {
            var user = _userManager.GetUserById(userId);
            var item = itemId.Equals(Guid.Empty)
                ? _libraryManager.GetUserRootFolder()
                : _libraryManager.GetItemById(itemId);

            var dtoOptions = new DtoOptions().AddClientFields(Request);
            var dtosExtras = item.GetExtras(new[] { ExtraType.Trailer })
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item))
                .ToArray();

            if (item is IHasTrailers hasTrailers)
            {
                var trailers = hasTrailers.LocalTrailers;
                var dtosTrailers = _dtoService.GetBaseItemDtos(trailers, dtoOptions, user, item);
                var allTrailers = new BaseItemDto[dtosExtras.Length + dtosTrailers.Count];
                dtosExtras.CopyTo(allTrailers, 0);
                dtosTrailers.CopyTo(allTrailers, dtosExtras.Length);
                return allTrailers;
            }

            return dtosExtras;
        }

        /// <summary>
        /// Gets special features for an item.
        /// </summary>
        [HttpGet("Users/{userId}/Items/{itemId}/SpecialFeatures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<BaseItemDto>> GetSpecialFeatures([FromRoute, Required] Guid userId, [FromRoute, Required] Guid itemId)
        user = _userManager.GetUserById(userId);
            var item = itemId.Equals(Guid.Empty)
                ? _libraryManager.GetUserRootFolder()
                : _libraryManager.GetItemById(itemId);

            var dtoOptions = new DtoOptions().AddClientFields(Request);
            return Ok(item
                .GetExtras(BaseItem.DisplayExtraTypes)
                .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item)));
        }

        /// <summary>
        /// Gets latest media.
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
            if (!isPlayed.HasValue)
{
    if (user.HidePlayedInLatest)
    {
        isPlayed = false;
    }
}

var dtoOptions = new DtoOptions { Fields = fields }
    .AddClientFields(Request)
    .AddAdditionalDtoOptions(enableImages, enableUserData, imageTypeLimit, enableImageTypes);

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

var dtos = list.Select(i =>
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

return Ok(dtos);
}

private async Task RefreshItemOnDemandIfNeeded(BaseItem item)
{
    if (item is Person)
    {
        var hasMetadata = !string.IsNullOrWhiteSpace(item.Overview) && item.HasImage(ImageType.Primary);
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

/// <summary>
/// Marks the favorite.
/// </summary>
private UserItemDataDto MarkFavorite(Guid userId, Guid itemId, bool isFavorite)
{
    var user = _userManager.GetUserById(userId);
    var item = itemId.Equals(Guid.Empty) ? _libraryManager.GetUserRootFolder() : _libraryManager.GetItemById(itemId);
    var data = _userDataRepository.GetUserData(user, item);
    data.IsFavorite = isFavorite;
    _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);
    return _userDataRepository.GetUserDataDto(item, user);
}

/// <summary>
/// Updates the user item rating.
/// </summary>
private UserItemDataDto UpdateUserItemRatingInternal(Guid userId, Guid itemId, bool? likes)
{
    var user = _userManager.GetUserById(userId);
    var item = itemId.Equals(Guid.Empty) ? _libraryManager.GetUserRootFolder() : _libraryManager.GetItemById(itemId);
    var data = _userDataRepository.GetUserData(user, item);
    data.Likes = likes;
    _userDataRepository.SaveUserData(user, item, data, UserDataSaveReason.UpdateUserRating, CancellationToken.None);
    return _userDataRepository.GetUserDataDto(item, user);
}
}