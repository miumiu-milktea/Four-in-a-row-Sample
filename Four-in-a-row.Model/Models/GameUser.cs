using System;

namespace Four_in_a_row.Models
{
    public enum GameUser : sbyte
    {
        First,
        Second,
    }

    public static class GameUserExtensions
    {
        public static GameUser GetNextUser(this GameUser user)
        {
            switch (user)
            {
                case GameUser.First:
                    return GameUser.Second;
                case GameUser.Second:
                    return GameUser.First;
                default:
                    throw new ArgumentException("invalid user.");
            }
        }

        public static GameUser GetPrevUser(this GameUser user)
        {
            return user.GetNextUser();
        }

        public static GameResult ToResult(this GameUser user)
        {
            switch (user)
            {
                case GameUser.First:
                    return GameResult.FirstWin;
                case GameUser.Second:
                    return GameResult.SecondWin;
                default:
                    throw new ArgumentException("invalid user.");
            }
        }
    }
}
