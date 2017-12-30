/**
 * @typedef {{Id: number, Data: string, Size: number, Name: string}} Attachment
 * @type {Attachment[]}
*/

function MsgFiles(chatId) {
    var $form = $('.form[data-chat-id="' + chatId + '"]'),
        self = this;
    this.input = $form.find('.att-input');
    this.label = $form.find('.att-btn');
    this.grid = $form.find('.attachments.grid').masonry();
    this.tmpl = $.templates('#attTmpl');
    this.files = [];
    this._initListeners();
}
/**
 * @param {string} fileName
*/
MsgFiles.prototype._isImage = function (fileName) {
    var imageArr = ['jpg', 'jpeg', 'png', 'gif'],
        arr = fileName.split('.'),
        ext = arr[arr.length - 1],
        isImage = false;

    imageArr.forEach(function (iName) {
        if (ext === iName) {
            isImage = true;
        }
    });
    return isImage;
}
/**
 * @param {Event} evt
*/
MsgFiles.prototype.read = function (evt) {
    var self = this,
        files = evt.target.files,
        tmpl = self.tmpl,
        grid = self.grid,
        index = self.files.length - 1;
    var loaded = 0;
    self.label.addClass('loading disabled');
    for (var i = 0, file; file = files[i]; i++) {
        index++;
        var reader = new FileReader();
        reader.onload = (function (_file, _index) {
            return function (e) {
                loaded++;
                self.files.push({
                    Id: _index,
                    Data: e.target.result,
                    Name: _file.name,
                    Size: _file.size,
                    Stream: _file
                });
                grid.addClass('active')
                    .append(tmpl.render({
                        Id: _index,
                        Name: _file.name,
                        Src: e.target.result,
                        IsImage: self._isImage(_file.name)
                    })).masonry('reloadItems').imagesLoaded(function () {
                        grid.masonry('layout');
                    });
                grid.find('.column:not(.initialized)>.image,.column:not(.initialized)>.segment').dimmer({
                    on: 'hover'
                }).addClass('initialized');
                if (loaded >= files.length)
                    self.label.removeClass('loading disabled');
            }
        })(file, index);
        reader.readAsDataURL(file);
        self.summarySize += file.size;
    }
    self.input.val('');
}
/**
 * @param {Event} ev
*/
MsgFiles.prototype.clickEvent = function (elem) {
    var removeIndex, files = this.files;
    for (var i in files) {
        if (files[i].Id === +elem.dataset.id) {
            removeIndex = i;
            break;
        }
    }
    files.splice(removeIndex, 1);
    this.grid.masonry('remove', elem).masonry('layout');
}
MsgFiles.prototype._initListeners = function () {
    var self = this;
    this.input.on('change', function (ev) {
        self.read(ev);
    });
    this.grid.on('click', '.column', function (ev) {
        self.clickEvent(this);
    }).on('removeComplete', function () {
        if (self.files.length === 0) {
            $(this).removeClass('active').find('.column').remove();
        }
    });
}