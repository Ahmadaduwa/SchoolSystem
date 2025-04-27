namespace SchoolSystem.Models.ViewModels
{
    public class DayViewModel
    {
        public string English { get; }
        public string Thai { get; }

        public DayViewModel(string english, string thai)
        {
            English = english;
            Thai = thai;
        }
    }
}
