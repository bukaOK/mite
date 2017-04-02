function initPassStrength(selector) {
    $(selector)
        .keyup(function () {
            var pass = $(this).val();
            if (pass != null && pass !== "") {
                var strength = passwordStrength(pass);
                $("#pass-strength").html("Сложность: " + strength);
                $("#pass-strength").show();
            } else {
                $("#pass-strength").hide();
            }
        });
}
function passwordStrength(pass) {
    var score = scorePassword(pass);
    if (score > 80)
        return "тяжелый";
    if (score > 60)
        return "хороший";
    if (score >= 30)
        return "средний";
    return "слабый";
}
function scorePassword(pass) {
    var score = 0;
    if (!pass)
        return score;

    // award every unique letter until 5 repetitions
    var letters = new Object();
    for (var i = 0; i < pass.length; i++) {
        letters[pass[i]] = (letters[pass[i]] || 0) + 1;
        score += 5.0 / letters[pass[i]];
    }

    // bonus points for mixing it up
    var variations = {
        digits: /\d/.test(pass),
        lower: /[a-z]/.test(pass),
        upper: /[A-Z]/.test(pass),
        nonWords: /\W/.test(pass),
    }

    var variationCount = 0;
    for (var check in variations) {
        variationCount += (variations[check] == true) ? 1 : 0;
    }
    score += (variationCount - 1) * 10;

    return parseInt(score);
}