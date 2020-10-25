using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace OOP_Lab1
{
    class LabCalculatorVisitor : LabCalculatorBaseVisitor<dynamic>
    {
        //таблиця ідентифікаторів (тут для прикладу)
        //в лабораторній роботі заміните на свою!!!!
       
        
        public static Dictionary<string, dynamic> tableIdentifier = new Dictionary<string, dynamic>();

        public override dynamic VisitCompileUnit(LabCalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expression());
        }

        public override dynamic VisitNumberExpr(LabCalculatorParser.NumberExprContext context)
        {
            var i = context.GetText();
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }

        //IdentifierExpr
        public override dynamic VisitIdentifierExpr(LabCalculatorParser.IdentifierExprContext context)
        {
            var result = context.GetText();
            dynamic value;
            //видобути значення змінної з таблиці
            if (tableIdentifier.TryGetValue(result.ToString(), out value))
            {
                return value;
            }
            else
            {
                throw new ArgumentException("Invalid Expression: {0}", result);
            }
        }

        public override dynamic VisitParenthesizedExpr(LabCalculatorParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }
        public override dynamic VisitAdditiveExpr(LabCalculatorParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.ADD)
            {
              
                return left + right;
            }
            else //LabCalculatorLexer.SUBTRACT
            {
 
                return left - right;
            }
        }

        public override dynamic VisitMultiplicativeExpr(LabCalculatorParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == LabCalculatorLexer.MULTIPLY)
            {
                return left * right;
            }
            else //LabCalculatorLexer.DIVIDE
            {
                if (right == 0) throw new DivideByZeroException("Помилка!!! Ділення на 0");
                return left / right;
            }
        }


        public override dynamic VisitBoolExpression([NotNull] LabCalculatorParser.BoolExpressionContext context)
        {
            var left = (double)WalkLeft(context);
            var right = (double)WalkRight(context);

            if(context.operatorToken.Type == LabCalculatorLexer.EQUAL)
            {
                return left == right;
            }
            else if (context.operatorToken.Type == LabCalculatorLexer.LESS)
            {
                return left < right;
            }
            else
            {
                return left > right;
            }
        }
        public override dynamic VisitNotExpression([NotNull] LabCalculatorParser.NotExpressionContext context)
        {
            var right = WalkLeft(context);
            return !right;
        }

        private dynamic WalkLeft(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(0));
        }

        private dynamic WalkRight(LabCalculatorParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<LabCalculatorParser.ExpressionContext>(1));
        }
        public override dynamic VisitDecIncExpression([NotNull] LabCalculatorParser.DecIncExpressionContext context)
        {
            var right = WalkLeft(context);
            if (context.operatorToken.Type == LabCalculatorLexer.INC)
            {
                return ++right;
            }
            else
            {
                return --right;
            }
        }
    }

}
