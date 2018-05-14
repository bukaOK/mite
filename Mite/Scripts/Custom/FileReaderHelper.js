/**
 * Хелпер для чтения файлов
 * @typedef {{progress: JQuery<HTMLElement>, imgWrapper: JQuery<HTMLElement>, field: JQuery<HTMLElement>}} ReaderSettings
 */
var FileReaderHelper = {
    /**
     * Читаем файл
     * @param {Event} evt событие чтения
     * @param {File} file
     * @param {ReaderSettings} settings
    */
    readFile: function (file, evt, settings) {
        var self = FileReaderHelper;
        if (!settings) {
            var $form = $(evt.target).parents('form'),
                $progress = $form.find('.ui.progress');
            settings = {
                imgWrapper: $form.find('.img-wrapper'),
                field: $form.find('[name="Content"]')
            };
            if (!settings.field.length) {
                settings.field = $form.find('[name=ImageSrc]')
            }
            if ($progress.length) {
                settings.progress = $progress;
            }
        }
        var reader = new FileReader();
        reader.onloadstart = function (ev) {
            if (settings.progress) {
                settings.progress.progress();
            }
        }
        reader.onprogress = function (ev) {
            if (settings.progress && ev.lengthComputable) {
                var percentLoaded = Math.round((ev.loaded / ev.total) * 100);
                settings.progress.progress('set percent', percentLoaded);
            }
        };
        reader.onloadend = function () {
            if (settings.progress) {
                settings.progress.progress('set percent', 100);
            }
        };
        reader.onload = function () {
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
     * @param {DragEvent} evt
    */
    dragOverHandler: function (evt) {
        evt.stopPropagation();
        evt.preventDefault();
        evt.dataTransfer.dropEffect = "copy";
    },
    /**
     * Когда переместили файл
     * @param {DragEvent} evt
     * @param {ReaderSettings} settings
    */
    dropHandler: function (evt, settings) {
        var self = FileReaderHelper;

        evt.stopPropagation();
        evt.preventDefault();
        var file = evt.dataTransfer.files[0];
        self.readFile(file, evt, settings);
    },
    /**
     * Загрузка файла
     * @param {Event} evt
     * @param {ReaderSettings} settings
     * @param {number} fileIndex индекс файла(если их несколько)
    */
    inputDownloadHandler: function (evt, settings, fileIndex) {
        var self = FileReaderHelper;
        var file = fileIndex ? evt.target.files[fileIndex] : evt.target.files[0];
        self.readFile(file, evt, settings);
    }
}