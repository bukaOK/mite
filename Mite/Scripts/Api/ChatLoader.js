function ChatLoader(chatId) {
    this._page = 1;
    this.ended = false;
    this.chat = $('.chat.feed[data-id="' + chatId + '"]');
    var self = this;
    this.scrollbar = new MiteScroll({
        wrap: this.chat.parent()[0],
        rollingY: true,
        thumbOffset: 8
    });
    //Поскольку скроллбар изменяет DOM и вытаскивает из него чат, нужно снова его инициализировать
    this.chat = $('.chat.feed[data-id="' + chatId + '"]');
    this.scrollbar.beginObserve(this.chat[0]);
    this.loader = $('.chat.feed[data-id="' + chatId + '"] .dot-loader-wrapper');
    this.loading = false;
    this.fixEnd = true;
    this._initListeners();
}
ChatLoader.prototype.loadNext = function () {
    if (this.ended)
        return;
    var self = this,
        initialized = self._page > 1,
        statuses = ChatMessages.Api.statuses;

    this.loading = true;
    this.fixEnd = !initialized;
    return $.ajax({
        dataType: 'json',
        data: {
            page: self._page,
            chatId: ChatMessages.chatId
        },
        url: '/api/message',
        success: function (resp) {
            self.ended = resp.ended;
            resp.data.forEach(function (msg, i, messages) {
                msg.images = [];
                msg.documents = [];
                if (msg.Attachments.length > 0) {
                    msg.Attachments.forEach(function (att, j) {
                        att.Index = j;
                        switch (att.Type) {
                            case 0:
                                msg.images.push(att);
                                break;
                            default:
                                msg.documents.push(att);
                                break;
                        }
                    });
                }
                var dateIso = msg.SendDate,
                    now = new Date();
                dateIso += dateIso[dateIso.length - 1] === 'Z' ? '' : 'Z';

                var date = new Date(dateIso);
                msg.Time = DateTimeHelper.toTimeString(date);
                msg.Date = DateTimeHelper.toDateString(date, 'short');
                msg.IsoDate = date;
                msg.Status = msg.Readed ? statuses.readed : statuses.sended;

                if (i === 0) {
                    var withYear = date.getFullYear() !== now.getFullYear();
                    msg.DividerDate = DateTimeHelper.toDateString(now, 'short') === msg.Date
                        ? 'Сегодня' : DateTimeHelper.toDateString(msg.IsoDate, 'long');
                } else if (initialized && i === messages.length - 1) {
                    var firstOldMsgDate = $('.chat .event').first()[0].dataset.date;
                    if (msg.Date === firstOldMsgDate) {
                        $('.chat .divider').first().remove();

                    }
                } else if (i > 0) {
                    var prevMsg = messages[i - 1];
                    if (prevMsg.Date !== msg.Date) {
                        msg.DividerDate = DateTimeHelper.toDateString(now, 'short') === msg.Date
                            ? 'Сегодня' : DateTimeHelper.toDateString(msg.IsoDate, 'long');
                    }
                }
            });
            var tmpl = $.templates('#messageTmpl'),
                html = tmpl.render(resp.data);
            self.loader.after(html);
            $('.event .extra.images:not(.listening)').addClass('listening')
                .click(function (ev) {
                    ev.stopPropagation();
                    ev.preventDefault();
                    $(this).lightGallery({
                        nextHtml: '<i class="angle big right icon"></i>',
                        prevHtml: '<i class="angle big left icon"></i>'
                    });
                });
            var scrollInner = $('.msg-chat-wrapper').find('.scroll-inner');
            if (!initialized)
                scrollInner.scrollTop(scrollInner[0].scrollHeight);
            self._page++;
        },
        error: function (jqXhr) {
            var errMsg = 'Внутренняя ошибка';
            if (jqXhr.status === 400) {
                errMsg = jqXhr.responseText;
            }
            $('#chat-form').form('add errors', [errMsg]);
        },
        complete: function () {
            self.loading = false;
            if (self.ended) {
                self.loader.removeClass('active');
            }
        }
    });
}
ChatLoader.prototype._initListeners = function () {
    var self = this;
    Scrolling.init(self.loader[0], function () {
        if (!self.ended && !self.loading) {
            self.loadNext();
        }
    }, self.chat.parent()[0]);
}