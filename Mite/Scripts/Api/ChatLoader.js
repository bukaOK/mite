/**
 * Класс загрузчика чата
 * @param {string} chatId id чата
 * @param {string} currentUserId id текущего пользователя
 * @param {boolean} public является ли чат открытым
 */
function ChatLoader(chatId, currentUserId, public) {
    this.public = public;
    this._page = 1;
    this.chatId = chatId;
    this.currentUserId = currentUserId;
    this.ended = false;
    this.chat = $('.chat.feed[data-id="' + chatId + '"]');
    var self = this;
    this.scrollbar = new PerfectScrollbar(this.chat.parent()[0]);
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
        statuses = ChatMessages.Api.statuses,
        wrap = self.chat.parent();

    this.loading = true;
    this.fixEnd = !initialized;
    return $.ajax({
        dataType: 'json',
        data: {
            page: self._page,
            chatId: self.chatId
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
                if (!self.public) {
                    if (msg.Sender.Id !== self.currentUserId && !msg.CurrentRead)
                        msg.Status = statuses.new;
                    else
                        msg.Status = msg.Readed ? statuses.readed : statuses.sended;
                }

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
                html = tmpl.render(resp.data),
                prevScrollPos = wrap[0].scrollHeight - wrap.scrollTop();

            self.loader.after(html);
            self.updateScrollState(!initialized, prevScrollPos);
            $('.event .extra.images:not(.initialized)').addClass('initialized')
                .click(function (ev) {
                    ev.stopPropagation();
                    ev.preventDefault();
                }).each(function (index, elem) {
                    $(elem).lightGallery({
                        nextHtml: '<i class="chevron large right icon"></i>',
                        prevHtml: '<i class="chevron large left icon"></i>'
                    });
                });
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
/**
 * Обновляем скроллбар
 * @param {boolean} scroll прокручивать ли вниз
*/
ChatLoader.prototype.updateScrollState = function (scroll, prevScrollPos) {
    var self = this,
        wrap = self.chat.parent();
    this.chat.imagesLoaded().progress(function () {
        self.scrollbar.update();
        if (scroll) {
            wrap.scrollTop(wrap[0].scrollHeight);
        } else {
            wrap.scrollTop(wrap[0].scrollHeight - prevScrollPos)
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