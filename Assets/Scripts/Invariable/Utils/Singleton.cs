namespace Invariable
{
    public class Singleton<T> where T : new()
    {
        private static T m_instance;

        public static T Instance
        {
            get
            {
                m_instance ??= new T();

                return m_instance;
            }
        }
    }
}