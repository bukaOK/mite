var FileReaderHelper = {
    /**
     * Читаем файл
     * @param {Event} evt событие чтения
    */
    readFile: function (file, evt) {
        var self = FileReaderHelper,
            settings = {
                saveBtn: $('#saveBtn'),
                imgWrapper: $(evt.target).parents('form').find('.img-wrapper'),
                field: $(evt.target).parents('form').find('[name="Content"]')
            };

        var reader = new FileReader();
        reader.onprogress = function () {
            settings.saveBtn.addClass('loading');
        };
        reader.onloadend = function () {
            settings.saveBtn.removeClass('loading');
        };
        reader.onload = function () {
            settings.saveBtn.show();
            settings.imgWrapper.html('<img src="' + reader.result + '" style="max-width: 100%"/>');
            settings.field.val(reader.result);
        };
        if (file.size / 1024 / 1024 > 30) {
            isImageLarge = true;
        }
        reader.readAsDataURL(file);
    },
    /**
     * Когда перемещаем файл мышкой и курсор над областью
     * @param {Event} evt
    */
    dragOverHandler: function (evt) {
        evt.stopPropagation();
        evt.preventDefault();
        evt.dataTransfer.dropEffect = "copy";
    },
    /**
     * Когда переместили файл
     * @param {Event} evt
    */
    dropHandler: function (evt) {
        var self = FileReaderHelper;

        evt.stopPropagation();
        evt.preventDefault();
        var file = evt.dataTransfer.files[0];
        self.readFile(file, evt);
    },
    /**
     * Нажали на кнопку загрузки
     * @param {Event} evt
    */
    inputDownloadHandler: function (evt) {
        var self = FileReaderHelper;

        var file = evt.target.files[0];
        self.readFile(file, evt);
    }
}