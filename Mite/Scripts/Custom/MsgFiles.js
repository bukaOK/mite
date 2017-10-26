var MsgFiles = {
    /**
     * @typedef {{Id: number, Name: string, Size: number[], Name: string}} Attachment
     * @type {Attachment[]}
    */
    files: [],
    grid: null,
    tmpl: null,
    input: null,
    summarySize: 0,
    /**
     * @param {string} fileName
    */
    _isImage: function (fileName) {
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
    },
    /**
     * Инициализируем обработчик загрузок
     * @param {string} inputId Id поля для загрузки файлов
    */
    init: function (inputId) {
        this.input = document.getElementById(inputId);
        this.input.onchange = MsgFiles.read;
        this.grid = $('.attachments.grid').masonry();
        this.tmpl = $.templates('#attTmpl');
    },
    /**
     * Читаем файлы из загрузки
     * @param {Event} evt событие изменения
    */
    read: function (evt) {
        var self = MsgFiles,
            files = evt.target.files,
            tmpl = self.tmpl,
            grid = self.grid,
            index = self.files.length - 1;

        for (var i = 0, file; file = files[i]; i++) {
            index++;
            var reader = new FileReader();

            reader.onload = (function (_file, _index) {
                return function (e) {
                    MsgFiles.files.push({
                        Id: index,
                        Data: e.target.result,
                        Name: _file.name,
                        Size: _file.size
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
                    }).addClass('initialized')
                }
            })(file, index);
            reader.readAsDataURL(file);
            self.summarySize += file.size;
        }
        self.input.value = '';
    },
    /**
     * Обработка нажатия на файл привязки
     * @param {HTMLElement} elem
    */
    clickEvent: function (elem) {
        var removeIndex, files = MsgFiles.files;
        for (var i in files) {
            if (files[i].Id === +elem.dataset.id) {
                removeIndex = i;
                break;
            }
        }
        files.splice(removeIndex, 1);
        this.grid.masonry('remove', elem).masonry('layout');
        if (files.length === 0) {
            this.grid.removeClass('active').find('.column').remove();
        }
    }
}