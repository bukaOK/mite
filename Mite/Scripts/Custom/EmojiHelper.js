var EmojiHelper = {
    initEmojies: function () {
        $('#emoji-btn').popup({
            inline: true,
            hoverable: true,
            position: 'top center'
        });
        $('.emoji .tabs .item').tab();
        $('.emoji .tab').each(function (index, elem) {
            new PerfectScrollbar(elem);
        });
        var emPs = new PerfectScrollbar('.em-wrapper', {
            useBothWheelAxes: true
        });
        $('.em-col').click(function () {
            EmojiHelper.addToArea('.dialog-area', this.childNodes[0].cloneNode());
        }).mousedown(function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
    },
    addToArea: function (textAreaSelector, elm) {
        var textArea = $(textAreaSelector).append(elm);
        if (!textArea.is(':focus')) {
            textArea.focus();
        }
        var sel, range;
        if (window.getSelection) {
            sel = window.getSelection();
            if (sel.getRangeAt && sel.rangeCount) {
                range = sel.getRangeAt(0);
                range.deleteContents();

                range.setStartAfter(elm);
                range.collapse(true);
                sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }
    }
}