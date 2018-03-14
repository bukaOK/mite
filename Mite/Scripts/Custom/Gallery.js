/**
 * Хелпер для галерей
 * @typedef {{ImageSrc: string, ImageSrc: string, Title: string, Id: string}} GalleryItem
 */
var MiteGallery = {
    init: function (respItems, initialIndex, baseLink, $elem) {
        var items = [];
        respItems.forEach(function (item) {
            items.push({
                thumb: item.ImageCompressed,
                src: item.ImageSrc,
                subHtml: '<a title="Перейти" href="' + baseLink + item.Id + '" class="ui inverted header">' + item.Title + '</a>'
            });
        });
        $elem.on('click', function (ev) {
            $(this).lightGallery({
                dynamic: true,
                dynamicEl: items,
                index: initialIndex,
                download: false,
                nextHtml: '<i class="angle big right icon"></i>',
                prevHtml: '<i class="angle big left icon"></i>'
            });
        });
    },
    /**
     * Галерея коллекции
     * @param {string} containerSel селектор контейнера
    */
    initCollection: function (containerSel) {
        $(containerSel).lightGallery({
            selector: '.col-item',
            download: false,
            nextHtml: '<i class="angle big right icon"></i>',
            prevHtml: '<i class="angle big left icon"></i>'
        });
    },
    /**
     * Галерея комикса
     * @param {string} containerSel селектор контейнера
    */
    initComics: function (containerSel) {
        $(containerSel).lightGallery({
            selector: '.comics-item',
            download: false,
            nextHtml: '<i class="angle big right icon"></i>',
            prevHtml: '<i class="angle big left icon"></i>'
        });
        $(containerSel).masonry().imagesLoaded(function () {
            $(containerSel).masonry('layout');
        });
    },
    /**
     * Галерея услуги
     * @param {GalleryItem[]} respItems
     * @param {JQuery<HTMLElement>} $img
    */
    initService: function (respItems, $img) {
        var items = [];
        respItems.forEach(function (item) {
            items.push({
                src: item.ImageSrc
            });
        });
        $img.on('click', function (ev) {
            $(this).lightGallery({
                dynamic: true,
                dynamicEl: items,
                index: 0,
                download: false,
                nextHtml: '<i class="angle big right icon"></i>',
                prevHtml: '<i class="angle big left icon"></i>'
            });
        });
    }
}