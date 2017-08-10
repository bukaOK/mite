var Settings = {
    init: function(){
        initSemanticSettings();
    },
    initSemanticSettings: function(){
        $.fn.dropdown.settings.message = {
            addResult: 'Добавить <b>{term}</b>',
            count: 'Выбрано {count}',
            maxSelections: 'Максимум {maxCount} вариантов',
            noResults: 'По запросу ничего не найдено'
        }
    }
};