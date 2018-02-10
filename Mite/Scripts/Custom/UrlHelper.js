var UrlHelper = {
    /**
     * Получить представление GET параметров в виде объекта
    */
    getObject: function () {
        var getObj = new Object(),
            params = decodeURIComponent(location.search.substr(1)).split('&');
        if (params.length === 0)
            return null;
        params.forEach(function (param) {
            var pair = param.split('=');
            getObj[pair[0]] = pair[1];
        });
        return getObj;
    }
}