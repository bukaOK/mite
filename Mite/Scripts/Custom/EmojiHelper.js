var EmojiHelper = {
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