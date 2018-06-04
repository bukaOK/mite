var PageSearch = {
    currentUserName: '',
    parseContent: function (searchContent, isAuthor) {
        var src = [];
        searchContent.forEach(function (cat) {
            if ((cat.forAuthors && isAuthor) || (cat.forClients && !isAuthor) || (!cat.forAuthors && !cat.forClients)) {
                cat.items.forEach(function (item) {
                    var pushItem = {
                        title: cat.baseTitle,
                        description: cat.baseTitle.toLowerCase() + '/' + item.title,
                        url: cat.baseUrl + item.action
                    };
                    if (cat.forAuthors || cat.forClients) {
                        pushItem.url = cat.baseUrl + PageSearch.currentUserName + '/' + item.action
                    }
                    src.push(pushItem);
                });
            }
        });
        return src;
    },
    init: function (selector, isAuthor) {
        var $search = $(selector);
        if ($search.length) {
            $.getJSON('/Content/searchContent.json', null, function (resp) {
                $search.search({
                    fullTextSearch: true,
                    source: PageSearch.parseContent(resp, isAuthor)
                });
            });
        }
    }
}