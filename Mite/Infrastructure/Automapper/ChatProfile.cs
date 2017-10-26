using AutoMapper;
using Mite.BLL.Helpers;
using Mite.DAL.Entities;
using Mite.Helpers;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            const string secKey = "AOgdKZhNixsujiLUWhqmeCYTqu7FWB5Q";
            CreateMap<Chat, ChatModel>()
                .ForMember(dest => dest.Emojies, opt => opt.UseValue(EmojiHelper.GetEmojies()));

            CreateMap<ChatMessage, ChatMessageModel>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src =>
                    CryptoHelper.Decrypt(src.Content, CryptoHelper.CreateKey(src.ChatId.ToString() + src.SenderId + secKey), src.IV)))
                .ForMember(dest => dest.Readed, opt => opt.MapFrom(src =>
                    src.Recipients.FirstOrDefault(x => x.Read == true && x.UserId != src.SenderId) != null))
                .ForMember(dest => dest.Recipients, opt => opt.Ignore());
            CreateMap<ChatMessageAttachment, MessageAttachmentModel>();
        }
    }
}