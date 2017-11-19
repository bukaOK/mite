/**
 * Имитация скролла
 * @typedef {{railOffset: number, wrap: string|HTMLElement, outer: string|HTMLElement, inner: string|HTMLElement, rollingX: boolean, rollingY: boolean, paddingRight: number, paddingBottom: number, marginRight: number, marginTop: number, marginBottom: number, minSize: number, thumbOffset: number}} Settings
 * @param {Settings} settings
 */
function MiteScroll(settings) {
    if (settings.wrap) {
        var content = '<div class="scroll-outer"><div class="scroll-rail-y"><div class="scroll-thumb-y"></div></div><div class="scroll-rail-x">' +
            '<div class="scroll-thumb-x"></div></div><div class="scroll-inner">' + $(settings.wrap).html() + '</div></div>';
        this.$scrollOuter = $(settings.wrap).html(content).children('.scroll-outer');
        this.$scrollInner = this.$scrollOuter.children('.scroll-inner');
    }
    else {
        var prependContent = '<div class="scroll-rail-y"><div class="scroll-thumb-y"></div></div><div class="scroll-rail-x">' +
            '<div class="scroll-thumb-x"></div></div>';
        this.$scrollOuter = $(settings.outer).addClass('scroll-outer').prepend(prependContent);
        this.$scrollInner = $(settings.inner).addClass('scroll-inner');
    }
    this.$scrollInner.css({
        'padding-bottom': settings.paddingBottom ? settings.paddingBottom : '20px',
        'margin-bottom': settings.marginBottom ? settings.marginBottom : '-30px',
        'padding-right': settings.paddingRight ? settings.paddingRight : '20px',
        'margin-right': settings.marginRight ? settings.marginRight : '-30px',
        'overflow-x': settings.rollingX ? 'scroll' : 'hidden',
        'overflow-y': settings.rollingY ? 'scroll' : 'hidden',
    });
    this.$scrollOuter.css('overflow', 'hidden');
    
    this.$railY = this.$scrollOuter.children('.scroll-rail-y');
    this.$railX = this.$scrollOuter.children('.scroll-rail-x');
    this.$thumbY = this.$railY.children('.scroll-thumb-y');
    this.$thumbX = this.$railX.children('.scroll-thumb-x');

    this.railOffset = settings.railOffset ? settings.railOffset : this.settings.railOffset;
    this.minThumbSize = settings.minSize ? settings.minSize : 10;
    this.thumbOffset = settings.thumbOffset ? settings.thumbOffset : 26;
    switch (ViewHelper.detectBrowser()) {
        case 'edge':
            this.thumbOffset = this.thumbOffset / 2;
            break;
    }

    this.observeElement = settings.observeElement ? settings.observeElement : this.$scrollInner[0];
    this.isVisible = {
        x: false,
        y: false
    };
    this.rolling = {
        x: settings.rollingX,
        y: settings.rollingY
    };
    this.$railY.outerHeight(this.$scrollInner.outerHeight() - this.railOffset * 2);
    
    this.initListeners();
    if (this.rolling.x)
        this.resizeScrollbar('x');
    else
        this.$railX.css('visibility', 'hidden');
    if (this.rolling.y)
        this.resizeScrollbar('y');
    else
        this.$railY.css('visibility', 'hidden');
}
MiteScroll.prototype.initListeners = function(){
    var self = this;
    this.observer = new MutationObserver(function (mutations) {
        if (self.rolling.y)
            self.resizeScrollbar('y');
        if (self.rolling.x)
            self.resizeScrollbar('x');
        if (self.settings.endFixed)
            self.$scrollInner.scrollTop(self.$scrollInner[0].scrollHeight + self.$scrollInner.outerHeight());
    });
    this.$scrollInner.scroll(function(ev){
        self.resizeScrollbar('y');
        self.resizeScrollbar('x');
    });
    this.$thumbX.mousedown(function(e){
        self.onDragX(e);
    });
    this.$thumbY.mousedown(function(e){
        self.onDragY(e);
    });
}
MiteScroll.prototype.beginObserve = function (target) {
    this.observer.observe(target, {
        childList: true
    });
}
/**
 * Когда нажали на вертикальный скролл
 * @param {MouseEvent} e
*/
MiteScroll.prototype.onDragY = function(e){
    e.preventDefault();
    var self = this,
        $thumb = this.$thumbY,
        eventOffset = e.pageY,
        dragOffset = eventOffset - $thumb[0].getBoundingClientRect().top;
    $(document).on('mousemove.mitescroll', function(e){
        self.onDragYMove(e, dragOffset);
    });
    $(document).on('mouseup.mitescroll', function(e){
        $(document).off('mousemove.mitescroll');
        $(document).off('mouseup.mitescroll');
    });
}
/**
 * Когда нажали на горизонтальный скролл
 * @param {MouseEvent} e
*/
MiteScroll.prototype.onDragX = function(e){
    e.preventDefault();
    var self = this,
        $thumb = this.$thumbX,
        eventOffset = e.pageX,
        dragOffset = eventOffset - $thumb[0].getBoundingClientRect().left;
    this.dragOffset.y = dragOffset;
    $(document).on('mousemove.mitescroll', function(e){
        self.onDragXMove(e, dragOffset);
    });
    $(document).on('mouseup.mitescroll', function(e){        
        $(document).off('mousemove.mitescroll');
        $(document).off('mouseup.mitescroll');
    });
}
/**
 * Нажали и переносят верт. скроллбар
 * @param {MouseEvent} e
 * @param {number} dragOffset
*/
MiteScroll.prototype.onDragYMove = function(e, dragOffset){
    e.preventDefault();
    var eventOffset = e.pageY,
        $rail = this.$railY,
        dragPos = eventOffset - $rail[0].getBoundingClientRect().top - dragOffset,
        dragPerc = dragPos / $rail.outerHeight(),
        scrollPos = dragPerc * this.$scrollInner[0].scrollHeight;
    this.$scrollInner.scrollTop(scrollPos);
}
/**
 * Нажали и переносят гориз. скроллбар
 * @param {MouseEvent} e
 * @param {number} dragOffset
*/
MiteScroll.prototype.onDragXMove = function(e, dragOffset){
    e.preventDefault();
    var eventOffset = e.pageX,
        $rail = this.$railX,
        dragPos = eventOffset - $rail[0].getBoundingClientRect().left - this.dragOffset.x,
        dragPerc = dragPos / $rail.outerWidth(),
        scrollPos = dragPerc * this.$scrollInner[0].scrollWidth;
    this.$scrollInner.scrollLeft(scrollPos);
}
/**
 * Изменяем размер скролла
 * @param {'x'|'y'} axis ось
*/
MiteScroll.prototype.resizeScrollbar = function(axis) {
    var $rail,
        $thumb,
        scrollOffset,
        contentSize,
        scrollbarSize;

    if (axis === 'x') {
        $rail = this.$railX;
        $thumb = this.$thumbX;
        scrollOffset = this.$scrollInner.scrollLeft(); // Either scrollTop() or scrollLeft().
        contentSize = this.$scrollInner[0].scrollWidth;
        scrollbarSize = this.$railX.width();
    } else { // 'y'
        $rail = this.$railY;
        $thumb = this.$thumbY;
        scrollOffset = this.$scrollInner.scrollTop(); // Either scrollTop() or scrollLeft().
        contentSize = this.$scrollInner[0].scrollHeight;
        scrollbarSize = this.$railY.height();
    }

    var scrollbarRatio = scrollbarSize / contentSize,
        scrollPourcent = scrollOffset / (contentSize - scrollbarSize),
        handleSize = Math.max(~~(scrollbarRatio * (scrollbarSize)) - 2, this.minThumbSize),
        handleOffset = ~~((scrollbarSize - this.thumbOffset - handleSize) * scrollPourcent + 2);

    this.isVisible[axis] = scrollbarSize < contentSize;

    if (this.isVisible[axis] && this.rolling[axis]) {
        $rail.css('visibility', 'visible');

        if (axis === 'x') {
            this.$thumbX.css({
                left: handleOffset,
                width: handleSize
            });
        } else {
            this.$thumbY.css({
                top: handleOffset,
                height: handleSize
            });
        }
    } else {
        $rail.css('visibility', 'hidden');
    }
}
/**
 * Настройки
*/
MiteScroll.prototype.settings = {
    inner: '.scroll-inner',
    outer: '.scroll-outer',
    railOffset: 6,
    endFixed: false
}