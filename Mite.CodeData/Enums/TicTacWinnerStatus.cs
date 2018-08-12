namespace Mite.CodeData.Enums
{
    public enum TicTacWinnerStatus
    {
        /// <summary>
        /// Победил крестик
        /// </summary>
        Cross = 0,
        /// <summary>
        /// Победил нолик
        /// </summary>
        Zero = 1,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error = 2,
        /// <summary>
        /// Ничья
        /// </summary>
        Draw = 3,
        /// <summary>
        /// Игра еще не закончена
        /// </summary>
        GameNotOver
    }
}
