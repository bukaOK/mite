var WatermarkHelper = {
    /**
     * Нарисовать водяной знак
     * @param {string} imgSelector
     * @param {string} watPath
     * @param {string} gravity
     */
    draw: function (imgSelector, watPath, gravity, imgSource) {
        var img = document.querySelector(imgSelector),
            wat = new Image(), originImg;
        if (imgSource) {
            originImg = new Image();
            originImg.src = imgSource;
        } else {
            originImg = img;
        }
        wat.onload = function () {
            var canvas = document.createElement('canvas'),
                ctx = canvas.getContext('2d'),
                w = img.width, h = img.height,
                wmW = wat.width, wmH = wat.height,
                pos = 0,
                gLeft, gTop;
            canvas.width = w;
            canvas.height = h;

            ctx.fillStyle = '#fff';
            ctx.fillRect(0, 0, w, h);
            ctx.drawImage(originImg, 0, 0, w, h);
            switch (gravity) { 
                case 'nw':
                    gLeft = pos;
                    gTop = pos;
                    break;
                case 'n':
                    gLeft = w / 2 - wmW / 2;
                    gTop = pos;
                    break;
                case 'ne':
                    gLeft = w - wmW - pos;
                    gTop = pos;
                    break;
                case 'w': 
                    gLeft = pos;
                    gTop = h / 2 - wmH / 2;
                    break;
                case 'e': 
                    gLeft = w - wmW - pos;
                    gTop = h / 2 - wmH / 2;
                    break;
                case 'sw':
                    gLeft = pos;
                    gTop = h - wmH - pos;
                    break;
                case 's': 
                    gLeft = w / 2 - wmW / 2;
                    gTop = h - wmH - pos;
                    break;
                case 'se':
                    gLeft = w - wmW - pos;
                    gTop = h - wmH - pos;
                    break;
                default:
                    gLeft = w / 2 - wmW / 2;
                    gTop = h / 2 - wmH / 2;
                    break;
            }
            ctx.drawImage(wat, gLeft, gTop, wmW, wmH);
            var dataUrl = canvas.toDataURL('image/jpeg');
            img.src = dataUrl;
        }
        wat.src = watPath;
    }
}