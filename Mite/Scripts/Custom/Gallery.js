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
                subHtml: '<p>' + elem.dataset.title + '</p>'
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
    }
}