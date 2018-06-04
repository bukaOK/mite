var Products = {
    BonusFile: {
        /**
         * @param {File} file
         */
        read: function (file, evt) {
            var self = this,
                $progress = $('#BonusProgress'),
                $format = $('#BonusFormat'),
                $field = $('#BonusBase64');

            var reader = new FileReader();
            reader.onloadstart = function (ev) {
                $progress.progress();
            }
            reader.onprogress = function (ev) {
                if (ev.lengthComputable) {
                    var percentLoaded = Math.round((ev.loaded / ev.total) * 100);
                    $progress.progress('set percent', percentLoaded);
                }
            };
            reader.onloadend = function () {
                $progress.progress('set percent', 100);
            };
            reader.onload = function () {
                $format.val(file.name.split('.').pop());
                $field.val(reader.result);
            };
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
        inputHandler: function (evt) {
            var file = evt.target.files[0];
            this.read(file, evt);
        }
    },
    Mvc: {
        buy: function (productId, btn) {
            var $btn = $(btn).addClass('loading disabled');
            return $.post('/products/buy/' + productId, function (resp) {
                if (resp.status === Settings.apiStatuses.success) {
                    iziToast.success({
                        title: 'Успех!',
                        message: 'Наслаждайтесь покупкой'
                    });
                } else {
                    iziToast.error({
                        title: 'Упс...',
                        message: resp.data[0]
                    });
                }
            }).fail(function () {
                iziToast.error({
                    title: 'Упс...',
                    message: resp.data[0]
                });
            }).always(function () {
                $btn.removeClass('loading disabled');
            });
        },
        buyAnonymous: function (email, btn) {
            var $btn = $(btn).addClass('loading disabled');
            return $.post('/products/buyanonymous?email=' + email, function () {
                if (resp.status === Settings.apiStatuses.success) {
                    iziToast.success({
                        title: 'Успех!',
                        message: 'Наслаждайтесь покупкой'
                    });
                }
            })
        }
    }
}