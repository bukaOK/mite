var ViewHelper = {
    /* Возвращает падеж слова по числу(2 комментария, 1 комментарий и тп)
       Входящие данные - объект с полями: 
       num - само число
       word1 - падеж для 1
       word2 - для 2,3,4
       word0 - для 0,5,6,7,8,9
    */
    getWordCase: function (num, word1, word2, word0) {
        if (num >= 10 && num <= 20) {
            return word0;
        }
        var num = num % 10;

        switch(num){
            case 0:
            default:
                return word0;
            case 1:
                return word1;
            case 2:
            case 3:
            case 4:
                return word2;
        }
    },
    /* 
       Преобразует разницу во времени в слова (два дня назад, неделю назад)
       publicTime - время публикации (в миллисекундах)
    */
    getPastTense: function (publicTime) {
        console.log(publicTime);
        var currentTime = new Date().getTime(),
        //Миллисекунды
            timeDiff = currentTime - publicTime,
            secsDiff = Math.floor(timeDiff / 1000),
            minutesDiff = Math.floor(secsDiff / 60),
            hoursDiff = Math.floor(minutesDiff / 60),
            daysDiff = Math.floor(hoursDiff / 24),
            weeksDiff = Math.floor(daysDiff / 7),
            monthsDiff = Math.floor(daysDiff / 30),
            yearsDiff = Math.floor(daysDiff / 365);

        console.log(yearsDiff);
        if(yearsDiff > 0){
            return yearsDiff + " " + this.getWordCase(yearsDiff, 'год', 'года', 'лет');
        }
        if(monthsDiff > 0){
            return monthsDiff + " " + this.getWordCase(monthsDiff, 'месяц', 'месяца', 'месяцев');
        }
        if(weeksDiff > 0){
            return weeksDiff + " " + this.getWordCase(weeksDiff, "неделя", "недели", "недель");
        }
        if(daysDiff > 0){
            return daysDiff + " " + this.getWordCase(daysDiff, "день", "дня", "дней");
        }
        if(hoursDiff > 0){
            return hoursDiff + " " + this.getWordCase(hoursDiff, "час", "часа", "часов");
        }
        if(minutesDiff > 0){
            return minutesDiff + " " + this.getWordCase(minutesDiff, "минута", "минуты", "минут");
        }
        if(timeDiff > 0){
            return "только что";
        }
        return null;
    },
    disableFormSubmitting: function (formSelector) {
        $(formSelector).keydown(function (ev) {
            if (ev.keyCode == 13) {
                event.preventDefault();
                return false;
            }
        });
    }
}