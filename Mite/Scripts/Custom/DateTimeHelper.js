var DateTimeHelper = {
    /**
     * Переводим в локальное время
     * @param {Date} date - дата
     * @returns {string}
    */
    toTimeString: function (date) {
        var hours = ('0' + date.getHours()).slice(-2),
            minutes = ('0' + date.getMinutes()).slice(-2);
        return hours + ':' + minutes;
    },
    /**
     * Переводим в локальную дату
     * @param {Date} date - дата
     * @param {('short'|'long')} type - тип даты
     * @param {boolean} [withYear=false]
     * @returns {string}
    */
    toDateString: function (date, type, withYear) {
        if (withYear === undefined || withYear === null) {
            withYear = false;
        }
        var months = ['января', 'февраля',
            'марта', 'апреля', 'мая',
            'июня', 'июля', 'августа',
            'сентября', 'октября', 'ноября', 'декабря'],
            day = date.getDate(),
            month,
            year = !withYear ? '' : '' + date.getFullYear();
        switch (type) {
            case 'long':
                month = months[date.getMonth()];
                if (withYear) {
                    year = ' ' + year;
                }
                return '' + day + ' ' + month + year;
            case 'short':
                if (withYear) {
                    year = '.' + year;
                }
                month = ('0' + date.getMonth()).slice(-2);
                return ('0' + day).slice(-2) + '.' + month + year;
            default:
                throw 'Unknown date type';
        }
    }
}