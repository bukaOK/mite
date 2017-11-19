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
                subHtml: '<a title="Перейти" href="' + baseLink + '"/' + item.Id + '" class="ui inverted header">' + item.Title + '</a>'
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
    initCollection: function (itemSelector) {
        var colItems = [];
        $(itemSelector).each(function (i, elem) {
            elem.dataset.index = i;
            colItems.push({
                src: $(elem).attr('src'),
                subHtml: '<h3 class="ui header">' + elem.dataset.title + '</h3>'
            });
        }).click(function () {
            $(this).lightGallery({
                dynamic: true,
                dynamicEl: colItems,
                index: this.dataset.index,
                download: false,
                nextHtml: '<i class="angle big right icon"></i>',
                prevHtml: '<i class="angle big left icon"></i>'
            });
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