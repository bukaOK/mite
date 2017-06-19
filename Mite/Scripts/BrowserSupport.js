var BrowserSupport = {
    css: {
        _cssPropertySupported: function(prop){
            return document.createElement('div').style[prop];
        },
        columns: function () {
            return this._cssPropertySupported('column-count') != undefined;
        },
    }
}