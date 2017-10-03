var Settings = {
    apiStatuses: {
        success: 0,
        error: 1,
        validationError: 2
    },
    init: function(){
        this.initSemanticSettings();
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