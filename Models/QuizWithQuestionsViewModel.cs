using System.Collections.Generic;
using EduQuiz_.Models;

namespace EduQuiz_.Models
{
    public class QuizWithQuestionsViewModel
    {
        public List<Quiz> Quizzes { get; set; }
        public Dictionary<int, List<Question>> QuestionsByQuizId { get; set; }

        public QuizWithQuestionsViewModel()
        {
            Quizzes = new List<Quiz>();
            QuestionsByQuizId = new Dictionary<int, List<Question>>();
        }
    }
}
