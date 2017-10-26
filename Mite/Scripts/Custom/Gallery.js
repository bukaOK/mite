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
    }
}