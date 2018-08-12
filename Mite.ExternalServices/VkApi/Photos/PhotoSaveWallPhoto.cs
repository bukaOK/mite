using Mite.ExternalServices.VkApi.Core;
using Mite.ExternalServices.VkApi.Objects;
using System.Collections.Generic;
using System.Net.Http;

namespace Mite.ExternalServices.VkApi.Photos
{
    public class PhotoSaveWallPhotoRequest : VkRequest<IEnumerable<Photo>>
    {
        public PhotoSaveWallPhotoRequest(HttpClient httpClient, string token) : base(httpClient, token)
        {
        }
        /// <summary>
        /// Идентификатор пользователя, на стену которого нужно сохранить фотографию
        /// </summary>
        [VkParam("user_id")]
        public string UserId { get; set; }
        /// <summary>
        /// Идентификатор сообщества, на стену которого нужно сохранить фотографию
        /// </summary>
        [VkParam("group_id")]
        public string GroupId { get; set; }
        /// <summary>
        /// Параметр, возвращаемый в результате загрузки фотографии на сервер. 
        /// </summary>
        [VkParam("photo")]
        public string Photo { get; set; }
        /// <summary>
        /// Параметр, возвращаемый в результате загрузки фотографии на сервер. 
        /// </summary>
        [VkParam("server")]
        public string Server { get; set; }
        /// <summary>
        /// Параметр, возвращаемый в результате загрузки фотографии на сервер. 
        /// </summary>
        [VkParam("hash")]
        public string Hash { get; set; }
        /// <summary>
        /// Географическая широта, заданная в градусах (от -90 до 90)
        /// </summary>
        [VkParam("latitude")]
        public double? Latitude { get; set; }
        /// <summary>
        /// Географическая долгота, заданная в градусах (от -180 до 180)
        /// </summary>
        [VkParam("longitude")]
        public double? Longitude { get; set; }
        /// <summary>
        /// Текст описания фотографии (максимум 2048 символов).
        /// </summary>
        [VkParam("caption")]
        public string Caption { get; set; }

        public override string Method => "photos.saveWallPhoto";
    }
}
