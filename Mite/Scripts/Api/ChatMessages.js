﻿/**
 * Статус сообщения
 * @constructor
 * @param {string} stClass
 * @param {string} title
 * @param {string} name
 */
function MessageStatus(stClass, title, name) {
    this.class = stClass;
    this.title = title;
    this.name = name;
}
var ChatMessages = {
    /**
     * Отправлен ли запрос на прочтение сообщений
    */
    readSended: true,
    /**
    * Изменяем статус сообщения
    * @typedef {{class: string, title: string, name: string}} MessageStatus
    * @param {JQuery<HTMLElement>} message элем. сообщения
    * @param {MessageStatus} status статус сообщения
    */
    changeStatus: function (message, status) {
        message[0].dataset.name = status.name;
        message.find('span.status').attr('title', status.title).children('.icon')
            .removeClass().addClass(status.class + ' violet icon');
    },
    initHub: function (chatId) {
        var chatHub = $.connection.chatHub;
        window.hubReady.done(function () {
            chatHub.server.readMessages(chatId);
        });

        chatHub.client.addMessage = function (msg) {
            msg.images = [];
            msg.documents = [];
            if (msg.Attachments !== null && msg.Attachments.length > 0) {
                msg.Attachments.forEach(function (att, j) {
                    att.Index = j;
                    if (att.Type === 0) {
                        msg.images.push(att);
                    } else {
                        msg.documents.push(att);
                    }
                });
            }
            var date = new Date();
            msg.Time = DateTimeHelper.toTimeString(date);
            msg.Date = DateTimeHelper.toDateString(date, 'short');
            msg.IsoDate = date;
            msg.Status = ChatMessages.Api.statuses.readed;

            var tmpl = $.templates('#messageTmpl');
            $('.chat').append(tmpl.render(msg));

            if (document.hasFocus()) {
                chatHub.server.readMessage(msg.Id);
            } else {
                ChatMessages.readSended = false;
            }
        }
        window.onfocus = function () {
            if (document.hasFocus() && !ChatMessages.readSended) {
                window.hubReady.done(function () {
                    chatHub.server.readMessages(chatId);
                    ChatMessages.readSended = true;
                });
            }
        }
        chatHub.client.readMessage = function (msgId) {
            ChatMessages.changeStatus($('.chat .event[data-id="' + msgId + '"]'), ChatMessages.Api.statuses.readed);
        }
        chatHub.client.readAll = function (chatId) {
            $('.chat[data-id="' + chatId + '"] .event[data-status="sended"]').each(function (index, elem) {
                ChatMessages.changeStatus($(elem), ChatMessages.Api.statuses.readed);
            });
        }
        //Обработка ввода
        chatHub.client.beginType = function (chatId, userName) {
            $('form[data-chat-id="' + chatId + '"] .chat-dl').addClass('active').siblings('.type-label').text(userName + ' пишет').show();
        }
        chatHub.client.endType = function (chatId) {
            $('form[data-chat-id="' + chatId + '"] .chat-dl').removeClass('active').siblings('.type-label').hide();
        }
    },
    /**
     * Обработка ввода сообщения
     * @param {HTMLElement} input
     * @param {string} userName
    */
    inputHandle: function (chatId, input) {
        window.hubReady.done(function () {
            setTimeout(function (lastVal, _chatId) {
                if (lastVal.length === input.innerText.length || input.innerText == '') {
                    $.connection.chatHub.server.typing(_chatId, false);
                    input.dataset.state = 'begin';
                }
            }, 600, input.innerText, chatId);
            var isBegin = input.dataset.state === 'begin';
            if (isBegin) {
                $.connection.chatHub.server.typing(chatId, true);
                input.dataset.state = 'process';
            }
        });
    },
    Events: {
        /**
         * Обработка нажатия на сообщение
         * @param {JQuery<HTMLElement>} message элемент сообщения
        */
        MessageClick: function (message) {
            message.toggleClass('marked').find('.msg-mark-wrap').toggleClass('active');
            if ($('.chat .event').hasClass('marked')) {
                $('.msg-actions').addClass('active');
            } else {
                $('.msg-actions').removeClass('active');
            }
        }
    },
    Api: {
        statuses: {
            wait: new MessageStatus('wait', 'Ожидает', 'wait'),
            sended: new MessageStatus('checkmark', 'Отправлено', 'sended'),
            readed: new MessageStatus('check circle outline', 'Прочитано', 'readed'),
            error: new MessageStatus('remove', 'Ошибка', 'error')
        },
        url: '/api/message',
        /**
         * Добавление сообщения
         * @typedef {{Id: string, UserName: string, AvatarSrc: string}} TargetUser
         * @param {string} chatId Id чата
         * @param {TargetUser} targetUser пользователь
         * @param {string} message контент сообщения
        */
        add: function (chatId, targetUser, message) {
            var self = ChatMessages.Api,
                $btn = $('#msgSendBtn').addClass('loading'),
                now = new Date(),
                formData = new FormData(),
                msgData = {
                    Time: DateTimeHelper.toTimeString(now),
                    Message: message,
                    Sender: {
                        UserName: targetUser.UserName,
                        AvatarSrc: targetUser.AvatarSrc
                    },
                    Status: self.statuses.sended,
                    Date: DateTimeHelper.toDateString(now, 'short')
                },
                tmpl = $.templates('#messageTmpl'),
                lastMsgDate = $('.chat .event').last()[0].dataset.date;

            if (lastMsgDate !== msgData.Date) {
                msgData.DividerDate = 'Сегодня';
            }
            var formData = {
                Recipient: {
                    Id: targetUser.Id
                },
                ChatId: chatId,
                Message: message,
                Attachments: []
            };
            MsgFiles.files.forEach(function (file) {
                formData.Attachments.push({
                    Src: file.Data,
                    Name: file.Name
                });
            });
            return new Promise(function (resolve, reject) {
                resolve(JSON.stringify(formData));
            }).then(function (data) {
                console.log('then');
                $.ajax({
                    url: self.url,
                    type: 'post',
                    dataType: 'json',
                    contentType: 'application/json',
                    data: data,
                    success: function (resp) {
                        msgData.images = [];
                        msgData.documents = [];
                        if (resp.Attachments !== null && resp.Attachments.length > 0) {
                            resp.Attachments.forEach(function (att, j) {
                                att.Index = j;
                                if (att.Type === 0) {
                                    msgData.images.push(att);
                                } else {
                                    msgData.documents.push(att);
                                }
                            });
                        }
                        msgData.Id = resp.Id;
                        $('#dialogArea').html('');
                        MsgFiles.files = [];
                        $('.attachments.grid').removeClass('active').html('');
                        $('.chat').append(tmpl.render(msgData));
                        window.hubReady.done(function () {
                            $.connection.chatHub.server.addMessage(resp);
                        });
                    },
                    error: function (jqXhr) {
                        var errMsg = 'Внутренняя ошибка';
                        if (jqXhr.status === 400) {
                            errMsg = jqXhr.responseText;
                        }
                        console.log(jqXhr);
                        $('#chat-form').form('add errors', [errMsg]);
                    },
                    complete: function () {
                        $btn.removeClass('loading');
                    }
                });
                }, function (ev) {
                    console.log(ev);
                });
        },
        /**
         * Удаляем выделенные сообщения
        */
        remove: function () {
            var ids = '';
            $('.event.marked').each(function (i, elem) {
                ids += 'ids=' + elem.dataset.id + '&';
            }).transition({
                animation: 'fly left',
                onHide: function () {
                    this.remove();
                    $('.chat .divider+.divider').prev().remove();
                    var lastDivider = $('.chat .divider:last-child');
                    if (lastDivider.next().length === 0) {
                        lastDivider.remove();
                    }
                }
                });
            return $.ajax({
                type: 'delete',
                url: '/api/message?' + ids,
                error: function () {
                    swal('Упс...', 'Ошибка при попытке удаления', 'error');
                }
            });
        }
    }
}