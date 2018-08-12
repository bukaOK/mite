/**
 * Крестики нолики
 * @param {JQuery<HTMLElement>} table
 * */
function TicTacToe(table) {
    var self = this;

    self.crossTmpl = $('#crossTmpl').html();
    self.zeroTmpl = $('#zeroTmpl').html();
    self.player = 0;
    self.table = table;

    self.winMoves = [
        [0, 1, 2], [3, 4, 5], [6, 7, 8],
        [0, 3, 6], [1, 4, 7], [2, 5, 8],
        [0, 4, 8], [2, 4, 6]
    ];
    self.crossPlayer = 1;
    self.zeroPlayer = 0;
    self.scores = {
        win: 10,
        lose: -10,
        draw: 0
    };
    self.initListeners();
    self.newGame();
}
TicTacToe.prototype.initListeners = function () {
    var self = this;
    $(self.table).find('.tt-cell').click(function (ev) {
        var $this = $(this);
        if ($this.hasClass('active')) {
            alert('Занято!');
            return;
        }
        var tmpl = self.player == self.crossPlayer ? self.crossTmpl : self.zeroTmpl;
        $this.html(tmpl).addClass('active');
        self.moveSequence.push($(this).data('cell'));

        self.pcMove();
    });
}
TicTacToe.prototype.newGame = function () {
    $(self.table).find('.tt-cell').html('').each(function (index, elem) {
        $(elem).removeClass('active');
    });
    this.moveSequence = [];
    this.gameOver = false;

    if (this.player === 1) {
        this.player = 0;
    } else {
        this.player = 1;
    }
}
/**
 * Находим массив с ходами
 * @param {number[]} arr 
 * @param {Array<string, number>} scores 
 * @param {number} player 
 * @param {number[]} current 
 * @returns Словарь типа scores[последовательность] = победность последовательности
 */
TicTacToe.prototype.possibleMoves = function (arr, scores, player, current) {
    if (!current)
        current = [];
    var currentCheck = this.moveSequence.concat(current);
    if (currentCheck.length >= 5) {
        var winner = this.searchWinner(currentCheck);
        if (winner != null) {
            var currentMove = currentCheck[this.moveSequence.length];
            if (winner == player) {
                scores[currentMove] += this.scores.win * currentCheck.length;
            } else if (winner != 2) {
                scores[currentMove] += this.scores.lose * currentCheck.length;
            } else {
                scores[currentMove] += this.scores.draw;
            }
            return scores;
        }
    }
    if (!arr.length)
        return scores;
    for (var i = 0; i < arr.length; i++) {
        var lst = arr.slice();
        lst.splice(i, 1);
        var newCurrent = current.slice();
        newCurrent.push(arr[i]);

        this.possibleMoves(lst, scores, player, newCurrent);
    }
    return scores;
}

TicTacToe.prototype.pcMove = function () {
    var self = this;

    var winner = self.searchWinner(self.moveSequence);
    var pcPlayer = self.player == self.crossPlayer ? self.zeroPlayer : self.crossPlayer;
    if (winner != null) {
        if (winner == self.player) {
            alert('Вы победили!');
        } else if (winner != 2) {
            alert('Вы проиграли!');
        } else {
            alert('Ничья!');
        }
        self.newGame();
        return;
    }
    var scores = [],
        availMoves = self.getFreeCells();
    for (var i = 0; i < 9; i++) {
        scores[i] = 0;
    }
    
    self.possibleMoves(availMoves, scores, pcPlayer);
    console.log(scores);
    var bestScore = -100000;
    bestMove = 0;
    
    scores.forEach(function (score, index) {
        if (score > bestScore && availMoves.indexOf(index) > -1) {
            bestScore = score;
            bestMove = index;
        }
    });

    var tmpl = self.player == self.crossPlayer ? self.zeroTmpl : self.crossTmpl;
    self.moveSequence.push(bestMove);
    self.table.find('[data-cell=' + bestMove + ']').html(tmpl).addClass('active');
}
/**
 * Ищем победителя в последовательности
 * @param {Array<number>} moves 
 * @returns 1 если крестик, 0 если нолик, 2 если ничья, null если игра не закончена
 */
TicTacToe.prototype.searchWinner = function (moves) {
    if (moves.length < 5)
        return null;
    var winMoves = this.winMoves,
        winner = moves.length < 9 ? null : 2,
        crossMoves = [],
        zeroMoves = [];
    for (var i = 0; i < moves.length; i++) {
        if (i % 2 === 0)
            crossMoves.push(moves[i]);
        else
            zeroMoves.push(moves[i]);
    }
    for (var i = 0; i < winMoves.length; i++) {
        if (winMoves[i].every(function (winMove) {
            return crossMoves.indexOf(winMove) > -1;
        })) {
            winner = this.crossPlayer;
            break;
        } else if (winMoves[i].every(function (winMove) {
            return zeroMoves.indexOf(winMove) > -1;
        })) {
            winner = this.zeroPlayer;
            break;
        }
    }
    return winner;
}
TicTacToe.prototype.getFreeCells = function () {
    var freeMoves = [];
    for (var i = 0; i < 9; i++) {
        if (this.moveSequence.indexOf(i) == -1) {
            freeMoves.push(i);
        }
    }
    return freeMoves;
}