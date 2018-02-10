using AutoMapper;
using Mite.BLL.Helpers;
using Mite.CodeData.Constants;
using Mite.CodeData.Constants.Automapper;
using Mite.DAL.DTO;
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
            CreateMap<ChatMember, ChatMemberModel>()
                .ForMember(dest => dest.CanExclude, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    var currentUserId = context.Items["currentUserId"];
                    return src.InviterId == (string)currentUserId;
                }));

            CreateMap<ChatMember, UserShortModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId));

            CreateMap<ChatModel, Chat>()
                .ForMember(dest => dest.MaxMembersCount, opt => opt.MapFrom(src => src.MaxMembersCount ?? 100))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ChatType))
                .ForMember(dest => dest.ImageSrcCompressed, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.MapFrom(src => src.Members.Count >= 1 ? src.Members[0].Id : null))
                .ForMember(dest => dest.ImageSrc, opt => opt.ResolveUsing((src, dest) =>
                {
                    if (!string.IsNullOrEmpty(src.ImageSrc))
                    {
                        var tuple = ImagesHelper.CreateImage(PathConstants.VirtualImageFolder, src.ImageSrc);
                        dest.ImageSrcCompressed = tuple.compressedVPath;
                        return tuple.vPath;
                    }
                    return null;
                }))
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members.Select(x => new ChatMember
                {
                    UserId = x.Id,
                    EnterDate = DateTime.UtcNow
                }).ToList()));

            CreateMap<Chat, ChatModel>()
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
                .ForMember(dest => dest.Emojies, opt => opt.UseValue(EmojiHelper.GetEmojies()));

            CreateMap<Chat, ShortChatModel>()
                .ForMember(dest => dest.NewMessagesCount, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    if(context.Items.TryGetValue("messagesCount", out object newCounts))
                    {
                        return (newCounts as IEnumerable<NewMessagesCountDTO>).FirstOrDefault(x => x.ChatId == src.Id)?.MessagesCount ?? 0;
                    }
                    return 0;
                }))
                .ForMember(dest => dest.ChatType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ImageSrc) 
                    ? "/Content/images/doubt-ava.png"
                    : string.IsNullOrEmpty(src.ImageSrcCompressed) ? src.ImageSrc : src.ImageSrcCompressed))
                .ForMember(dest => dest.LastMessage, opt => opt.ResolveUsing(src =>
                {
                    var lastMsg = src.Messages?[0];
                    if(lastMsg != null)
                        lastMsg.Chat = null;
                    return lastMsg;
                }));
            CreateMap<PublicChatDTO, ShortChatModel>()
                .ForMember(dest => dest.ImageSrc, opt => opt.MapFrom(src => src.ImageSrcCompressed ?? src.ImageSrc));
            CreateMap<ChatMessage, ChatMessageModel>()
                .ForMember(dest => dest.CurrentRead, opt => opt.ResolveUsing((src, dest, val, context) =>
                {
                    if(context.Items.TryGetValue("currentUserId", out object currentUserId))
                        //Пользователя не может быть в получателях из-за того, что он вошел в чат после отправки сообщения
                        return !src.Recipients.Any(x => x.UserId == currentUserId as string) || 
                            src.Recipients.Any(x => x.Read == true && x.UserId == currentUserId as string);
                    return false;
                }))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src =>
                    CryptoHelper.Decrypt(src.Content, CryptoHelper.CreateKey(src.ChatId.ToString() + src.SenderId + secKey), src.IV)))
                .ForMember(dest => dest.Readed, opt => opt.MapFrom(src => src.Recipients != null &&
                     src.Recipients.Any(x => x.Read == true && x.UserId != src.SenderId)))
                .ForMember(dest => dest.Recipients, opt => opt.Ignore());
            CreateMap<ChatMessageAttachment, MessageAttachmentModel>();
        }
    }
}