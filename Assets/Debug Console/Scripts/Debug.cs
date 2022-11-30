using System.Collections.Generic;
using System.Diagnostics;

namespace SampleCode
{
    public static class Debug
    {
        [Conditional("UNITY_DEBUG")]
        public static void Log(int newLine_)
        {
            for (int i = 0; i < newLine_; ++i)
            {
                SampleCode.Log.Push(string.Empty);
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(string message_, int newLine_ = 0)
        {
            for (int i = 0; i < newLine_; ++i)
            {
                SampleCode.Log.Push(string.Empty);
            }

            string message = "[Debug] " + message_;

#if UNITY_EDITOR
            SampleCode.Log.Push(message);
#else
            UnityEngine.Debug.unityLogger.Log("EH.Debug", message);
#endif
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(BattleTurn turn_, string message_ = "")
        {
            if (turn_ != null)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("<color=red>[BattleTurn]</color> ");
                builder.Append(string.Format(message_ + " {0}", turn_.ToString()));

#if UNITY_EDITOR
                SampleCode.Log.Push(builder.ToString());
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", builder.ToString());
#endif
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(BattleTurnManager manager_, string message_)
        {
            if (manager_ != null)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("<color=red>[BattleTurnManager]</color> ");
                builder.AppendLine(message_);

                builder.AppendLine("CurrentTurn[ " + (manager_.CurrentTurn != null ? manager_.CurrentTurn.ToString() : "<color=red>null</color>") + " ]");

                Queue<BattleTurn>.Enumerator enumerator;
                Queue<BattleTurn> queue = manager_.EventsRef;
                //if (queue.Count > 0)
                {
                    builder.AppendLine("Event Queue");
                    builder.AppendLine("[");
                    enumerator = queue.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        builder.AppendLine(enumerator.Current != null ? enumerator.Current.ToString() : "null");
                    }
                    builder.AppendLine("]");
                }

                builder.AppendLine("Base Queue");
                builder.AppendLine("[");
                enumerator = manager_.TurnsRef.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    builder.AppendLine(enumerator.Current != null ? enumerator.Current.ToString() : "null");
                }
                builder.AppendLine("]");

                builder.AppendLine("History Queue");
                builder.AppendLine("[");
                enumerator = manager_.HistoriesRef.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    builder.AppendLine(enumerator.Current != null ? enumerator.Current.ToString() : "null");
                }
                builder.AppendLine("]");

#if UNITY_EDITOR
                SampleCode.Log.Push(builder.ToString());
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", builder.ToString());
#endif
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(BattleTriggerManager manager_, string message_, int newLine_ = 0)
        {
            if (manager_ != null)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("<color=yellow>[BattleTriggerManager]</color> ");
                builder.AppendLine(message_);

                builder.AppendLine("Queue");
                builder.AppendLine("[");

                List<BattleTrigger>.Enumerator enumerator = manager_.Triggers.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    builder.AppendLine(enumerator.Current.ToString());
                }
                builder.AppendLine("]");

                for (int i = 0; i < newLine_; ++i)
                {
                    SampleCode.Log.Push(string.Empty);
                }

#if UNITY_EDITOR
                SampleCode.Log.Push(builder.ToString());
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", builder.ToString());
#endif
            }
        }

        /// <summary>
        /// 캐릭터 스테이트 로그
        /// </summary>
        /// <param name="character_"></param>
        /// <param name="nextState_"></param>

        [Conditional("UNITY_DEBUG")]
        public static void Log(UnitBattleActor character_, string nextState_, bool force_)
        {
            if (character_ != null)
            {
                string message = (force_ ? "[ForceChangeState] " : "[ChangeState] ") + string.Format("{0}({1}) {2} -> {3}", character_.name, character_.TeamCodeDebugText, character_.GetCurrentStateName(), nextState_);

#if UNITY_EDITOR
                SampleCode.Log.Push(message);
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", message);
#endif
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(BattleProcessor processor_, string message_ = "")
        {
            if (processor_ != null)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("<color=red>[BattleProcessor]</color> ");
                builder.AppendLine(message_);

                builder.AppendFormat("State: {0} ", processor_.CurrentState);
                builder.AppendFormat("Wave: {0}/{1} ", processor_.CurrentWave + 1, processor_.TotalWave);
                builder.AppendFormat("RoundTurn: {0} ", processor_.BattleTurnManager.RoundTurn + 1);
                builder.AppendFormat("RealTurn: {0} ", processor_.BattleTurnManager.RealTurn + 1);
                builder.AppendFormat("GlobalTurn: {0} ", processor_.BattleTurnManager.GlobalTurn + 1);

#if UNITY_EDITOR
                SampleCode.Log.Push(builder.ToString());
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", builder.ToString());
#endif
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void Log(BattleJudgementManager manager_, string message_ = "")
        {
            if (manager_ != null)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();

                builder.Append("<color=red>[JudgementResult]</color> ");
                builder.AppendLine(message_);

                List<BattleJudgement> results = manager_.Results;

                int i, count = results.Count;

                for (i = 0; i < count; ++i)
                {
                    builder.AppendLine(results[i].ToString());
                }

#if UNITY_EDITOR
                SampleCode.Log.Push(builder.ToString());
#else
                UnityEngine.Debug.unityLogger.Log("EH.Debug", builder.ToString());
#endif
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void LogError(string message_)
        {
            //UnityEditor.EditorApplication.isPaused = true;

#if UNITY_EDITOR
            SampleCode.Log.Push($"<color=red>{message_}</color>", SampleCode.Log.Category.Error);
#endif
            UnityEngine.Debug.unityLogger.LogError("EH.Debug", message_);
        }

        [Conditional("UNITY_DEBUG")]
        public static void LogUniqueError(string message_)
        {
            //UnityEditor.EditorApplication.isPaused = true;

#if UNITY_EDITOR
            if (SampleCode.Log.UniquePush($"<color=red>{message_}</color>", SampleCode.Log.Category.Error))
#endif
            {
                UnityEngine.Debug.unityLogger.LogError("EH.Debug", message_);
            }
        }

        [Conditional("UNITY_DEBUG")]
        public static void LogLua(string message_)
        {
#if UNITY_EDITOR
            SampleCode.Log.Push(message_, SampleCode.Log.Category.Lua);
#else
            UnityEngine.Debug.unityLogger.LogError("EH.Debug", message_);
#endif
        }
    }
}
