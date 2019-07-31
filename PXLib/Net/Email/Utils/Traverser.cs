using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PXLib.Net.Email
{


    public class FindFirstMessagePartWithMediaType
    {
        public MessagePart VisitMessage(PopMessage message, string question)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            return this.VisitMessagePart(message.MessagePart, question);
        }
        public MessagePart VisitMessagePart(MessagePart messagePart, string question)
        {
            if (messagePart == null)
            {
                throw new ArgumentNullException("messagePart");
            }
            MessagePart result2;
            if (messagePart.ContentType.MediaType.Equals(question, StringComparison.OrdinalIgnoreCase))
            {
                result2 = messagePart;
            }
            else
            {
                if (messagePart.IsMultiPart)
                {
                    foreach (MessagePart part in messagePart.MessageParts)
                    {
                        MessagePart result = this.VisitMessagePart(part, question);
                        if (result != null)
                        {
                            result2 = result;
                            return result2;
                        }
                    }
                }
                result2 = null;
            }
            return result2;
        }

    }
    public abstract class AnswerMessageTraverser<T>
    {
       public T VisitMessage(PopMessage message)
       {
           if (message == null)
               throw new ArgumentNullException("message");

           return VisitMessagePart(message.MessagePart);
       }
       public T VisitMessagePart(MessagePart messagePart)
       {
           if (messagePart == null)
               throw new ArgumentNullException("messagePart");

           if (messagePart.IsMultiPart)
           {
               List<T> leafAnswers = new List<T>(messagePart.MessageParts.Count);
               foreach (MessagePart part in messagePart.MessageParts)
               {
                   leafAnswers.Add(VisitMessagePart(part));
               }
               return MergeLeafAnswers(leafAnswers);
           }

           return CaseLeaf(messagePart);
       }

       protected abstract T CaseLeaf(MessagePart messagePart);

       /// <summary>
       /// For a concrete implementation, when a MultiPart <see cref="MessagePart"/> has fetched it's answers from it's children, these
       /// answers needs to be merged. This is the responsibility of this method.
       /// </summary>
       /// <param name="leafAnswers">The answer that the leafs gave</param>
       /// <returns>A merged answer</returns>
       protected abstract T MergeLeafAnswers(List<T> leafAnswers);
    }

   internal class TextVersionFinder : MultipleMessagePartFinder
   {
       protected override List<MessagePart> CaseLeaf(MessagePart messagePart)
       {
           if (messagePart == null)
               throw new ArgumentNullException("messagePart");

           // Maximum space needed is one
           List<MessagePart> leafAnswer = new List<MessagePart>(1);

           if (messagePart.IsText)
               leafAnswer.Add(messagePart);

           return leafAnswer;
       }
   }
   public abstract class MultipleMessagePartFinder : AnswerMessageTraverser<List<MessagePart>>
   {
       /// <summary>
       /// Adds all the <paramref name="leafAnswers"/> in one big answer
       /// </summary>
       /// <param name="leafAnswers">The answers to merge</param>
       /// <returns>A list with has all the elements in the <paramref name="leafAnswers"/> lists</returns>
       /// <exception cref="ArgumentNullException">if <paramref name="leafAnswers"/> is <see langword="null"/></exception>
       protected override List<MessagePart> MergeLeafAnswers(List<List<MessagePart>> leafAnswers)
       {
           if (leafAnswers == null)
               throw new ArgumentNullException("leafAnswers");

           // We simply create a list with all the answer generated from the leaves
           List<MessagePart> mergedResults = new List<MessagePart>();

           foreach (List<MessagePart> leafAnswer in leafAnswers)
           {
               mergedResults.AddRange(leafAnswer);
           }

           return mergedResults;
       }
   }
}
