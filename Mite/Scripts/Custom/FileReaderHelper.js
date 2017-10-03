var FileReaderHelper = {
    readFile: function (file, evt) {
        var self = FileReaderHelper;
        if (self.settings === undefined) {
            self.settings = {
                saveBtn: $('#save-btn'),
                imgWrapper: $('#img-wrapper'),
                field: $('#Content')
            };
        }

        var reader = new FileReader();
        reader.onprogress = function () {
            $(evt).addClass('loading');
        };
        reader.onloadend = function () {
            $(evt).removeClass('loading');
        };
        reader.onload = function () {
            self.settings.saveBtn.show();
            self.settings.imgWrapper.html('<img src="' + reader.result + '" style="max-width: 100%"/>');
            self.settings.field.val(reader.result);
        };
        if (file.size / 1024 / 1024 > 30) {
            isImageLarge = true;
        }
        reader.readAsDataURL(file);
    },
    //Когда перемещаем файл мышкой и курсор над областью
    dragOverHandler: function (evt) {
        evt.stopPropagation();
        evt.preventDefault();
        evt.dataTransfer.dropEffect = "copy";
    },
    //Когда переместили файл
    dropHandler: function (evt) {
        var self = FileReaderHelper;

        evt.stopPropagation();
        evt.preventDefault();
        var file = evt.dataTransfer.files[0];
        self.readFile(file, evt);
    },
    inputDownloadHandler: function (evt) {
        var self = FileReaderHelper;

        var file = evt.target.files[0];
        self.readFile(file, evt);
    }
}