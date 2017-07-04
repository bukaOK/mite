function MiteApi(url, data){
    this.errors = {
        500: 'Ошибка сервера.',
        404: 'URL не найден.'
    };
    this.method = method;
    this.url = url;
    if (data.tagName == undefined || data.nodeName == undefined) {
        this.data = new FormData();
        for (var prop in data) {
            this.data.append(prop, data[prop]);
        }
    } else {
        this.data = new FormData(data);
    }
    this.jsonResponse = false;
}
MiteApi.prototype.getAsync = function () {
    
}
MiteApi.prototype.postAsync = function () {

}
MiteApi.prototype._parseResponse = function(text, contentType) {
    if (contentType == 'application/json') {
        return JSON.parse(text);
    }
    return text;
}
MiteApi.prototype._sendWithUrl = function (method) {
    var self = this;

    var xhr = new XMLHttpRequest();
    var urlData = '?'
    for (var prop in this.data) {
        urlData += prop + '=' + data[prop];
        urlData += '&';
    }
    xhr.open(method, this.url + urlData, true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState != 4) {
            return;
        }
        var resp = self._parseResponse(xhr.responseText, xhr.getResponseHeader('Content-Type'));
        if (xhr.status == 200) {
            self.onSuccess(resp);
        } else {
            self.onError(resp);
        }
        self.onComplete(resp);
    }
    xhr.send()
}
MiteApi.prototype._sendWithBody = function (method) {
    var self = this;

    var xhr = new XMLHttpRequest();
    var formData;
    if (this.data.tagName == undefined || this.data.nodeName == undefined) {
        formData = new FormData()
        for (var prop in this.data) {
            urlData += prop + '=' + data[prop];
            urlData += '&';
        }
    }
}
MiteApi.prototype.onComplete = function (resp) { }
MiteApi.prototype.onSuccess = function (resp) { }
MiteApi.prototype.onError = function (resp) { }
