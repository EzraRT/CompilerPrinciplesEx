using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
namespace ex2
{

    //定义文法类，保存文法个数和记录所有文法

    public class Action
    {
        public int step { get; set; }
        public string stack { get; set; }
        public string input { get; set; }
        public string production { get; set; }
        public string action { get; set; }
        public string describe { get; set; }
        public Action(int _step, string _stack, string _input, string _production, string _action, string _describe)
        {
            step = _step;
            stack = _stack;
            input = _input;
            production = _production;
            action = _action;
            describe = _describe;
        }
        public override string ToString()=> string.Format("{0,-3:D} {1,-12} {2,-15} {3,-15} {4,-15} {5,-15}\n", step, stack, input, production, action, describe);

    };
    class Grammar
    {
        const int N = 50;
        //保存所有文法
        public ArrayList[,] grammarTable = new ArrayList[N, N];
        //保存终结字符
        public char[] terminalChar = new char[N];
        //保存终结字符的个数
        public int terNum;
        //保存每行的产生式的个数
        public int[] countEachRow = new int[N];
        //定义文法数量
        public int count;

        public Grammar()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    grammarTable[i, j] = new ArrayList();
                }
            }
            terNum = 0;
        }
    };
    //保存每个非终结符的FIRST集合
    class FIRST
    {
        const int N = 50;
        //保存每个非终结符的FIRST集合
        public SortedSet<char>[] First = new SortedSet<char>[N];
        //保存非终结符
        public char[] nonTerminal = new char[N];
        //保存是否计算出相应的FIRST集
        public bool[] flag = new bool[N];
        //保存已计算FIRST集的个数
        public int calCount;

        public FIRST()
        {
            for (int i = 0; i < N; i++)
            {
                First[i] = new SortedSet<char>();
            }
            calCount = 0;
        }
    };
    class Position
    {
        public int x;
        public int y;
        public Position()
        {
            x = -1;
            y = -1;
        }
        public Position(int a, int b)
        {
            x = a;
            y = b;
        }
    };
    //保存每个非终结符的FOLLOW集合
    class FOLLOW
    {
        const int N = 50;
        //保存每个非终结符的FOLLOW集合
        public SortedSet<char>[] Follow = new SortedSet<char>[N];
        //保存非终结符
        public char[] nonTerminal = new char[N];
        //保存是否计算出相应的FOLLOW集
        public bool[] flag = new bool[N];
        //保存已计算Follow集的个数
        public int calCount;
        //保存产生式的索引
        public ArrayList[] position = new ArrayList[N];

        public FOLLOW()
        {
            for (int i = 0; i < N; i++)
            {
                Follow[i] = new SortedSet<char>();
                position[i] = new ArrayList();
            }
            calCount = 0;
        }
    };

    class Analyser
    {
        Action[] finalResult;
        const int N = 50;
        //语法集
        public Grammar grammar;
        //FIRST集
        public FIRST first;
        //FOLLOW集
        public FOLLOW follow;
        //预测分析表
        public string[,] analyseTable = new string[N, N];
        private string input;

        public Analyser(string[] grammars)
        {
            grammar = new Grammar();
            first = new FIRST();
            follow = new FOLLOW();
            readGrammar(grammars);
            calFIRSTSet();
            getFollowSet();
            bulidAnalyseTable();
        }
        public void analysis(string str)
        {
            List<Action> actions = new List<Action>();
            //将输入的字符串转化为字符数组
            char[] buf = new char[100];
            Array.Copy(str.ToCharArray(), buf, str.Length);
            //计算字符的数目
            int count = str.Length;
            buf[str.Length] = '#';
            //定义一个分析栈
            Stack<char> analyseStack = new Stack<char>();
            //把'#'和文法开始符号入栈
            analyseStack.Push('#');
            analyseStack.Push((char)grammar.grammarTable[0, 0][0]);
            ArrayList vec = new ArrayList();
            vec.Add('#');
            vec.Add(grammar.grammarTable[0, 0][0]);
            //把第一个字符读入a中
            char a = buf[0];
            //记录步骤
            int step = 0;
            actions.Add(new Action(step, veToString(vec), toString(buf, 0, count - 1), " ", "初始化", " "));
            //buf[]的索引
            int index = 0;
            bool flag = true;
            while (flag)
            {
                char ch = '\0';
                if (analyseStack.Count != 0)
                {
                    ch = analyseStack.Pop();
                    vec.RemoveAt(vec.Count - 1);
                }
                if (isTerminal(ch) && ch != '#')
                {
                    if (ch == a)
                    {
                        index++;
                        a = buf[index];
                        step++;
                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", "GETNEXT(" + ch + ")", " "));
                    }
                    else
                    {
                        //出错
                        step++;
                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", "pop", "错误，栈顶终结符与输入符号不匹配"));
                    }
                }
                else if (ch == '#')
                {
                    if (ch == a)
                    {
                        flag = false;
                    }
                    else
                    {
                        //出错
                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", " ", "错误"));
                        finalResult = actions.ToArray();
                        return;
                    }
                }
                else if (isNonTerminal(ch))
                {
                    string tempstr = analyseTable[getNonTerminalIndex(ch), getTerminalIndex(a)];
                    //如果产生式不为空,且不为空字
                    if (tempstr != null && tempstr != "@" && tempstr.Length != 0 && tempstr != "synch")
                    {

                        int strSize = tempstr.Length;
                        for (int i = strSize - 1; i >= 0; --i)
                        {
                            analyseStack.Push(tempstr[i]);
                            vec.Add(tempstr[i]);
                        }

                        step++;

                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), ch + "->" + tempstr, "POP,PUSH(" + new string(tempstr.Reverse().ToArray()) + ")", " "));

                    }
                    //如果[M,a]为空,则跳过输入符号a
                    else if (tempstr.Length == 0)
                    {
                        //出错
                        index++;
                        a = buf[index];
                        step++;
                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", " ", "出错，跳过"));
                    }
                    else if (tempstr == "synch")
                    {
                        //如果栈顶为文法开始符号,跳过输入符号
                        if (ch.Equals(grammar.grammarTable[0, 0][0]))
                        {
                            index++;
                            a = buf[index];
                            //文法开始符号入栈
                            analyseStack.Push(ch);
                            vec.Add(ch);
                            step++;
                            actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", " ", "出错，跳过"));
                        }
                        else
                        {
                            step++;
                            actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", " ", "错,M[" + ch + "," + a + "]=synch" + "," + ch + "已弹出栈"));
                        }
                    }
                    //若为空字，什么也不做
                    else
                    {
                        step++;
                        actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), ch + "->" + "@", " ", " "));
                    }
                }
                else
                {
                    //出错
                    actions.Add(new Action(step, veToString(vec), toString(buf, index, count - 1), " ", " ", "错误"));
                    finalResult = actions.ToArray();
                    return;
                }
            }
            finalResult = actions.ToArray();
        }

        public Action[] getResult()
        {
            return finalResult;
        }

        void readGrammar(string[] grammars)
        {
            //把第i行文法转换为字符数组
            int index = 0;
            //临时保存非终结字符
            SortedSet<char> ter = new SortedSet<char>();
            foreach (var gram in grammars)
            {
                int i = 0;
                int count = 0;
                var temparr = grammar.grammarTable[index, count].Add(gram[i]);
                //略去"->"
                i += 3;
                //检测是否到边界
                while (i < gram.Length)
                {
                    //如果检测到"|"
                    if ((int)gram[i] == 124)
                    {
                        count++;
                        i++;
                        //保存起始字符
                        grammar.grammarTable[index, count].Add(gram[0]);
                        //保存产生式的每个字符
                        grammar.grammarTable[index, count].Add(gram[i]);
                        //如果是终结字符则保存
                        if (isTerminal(gram[i]))
                        {
                            ter.Add(gram[i]);
                        }
                        i++;
                    }
                    else
                    {
                        //保存产生式的每个字符
                        grammar.grammarTable[index, count].Add(gram[i]);
                        //如果是终结字符则保存
                        if (isTerminal(gram[i]))
                        {
                            ter.Add(gram[i]);
                        }
                        i++;
                    }
                }
                grammar.countEachRow[index] = count + 1;
                index++;
            }
            //保留文法个数
            grammar.count = index;
            //保存终结字符
            foreach (var ch in ter)
            {
                grammar.terminalChar[grammar.terNum] = ch;
                grammar.terNum++;
            }
            //加入特殊符号"#"，
            grammar.terminalChar[grammar.terNum] = '#';
            grammar.terNum++;
        }

        void calFIRSTSet()
        {
            while (reloadCalCount() != grammar.count)
            {
                //扫描每一个产生式
                for (int i = 0; i < grammar.count; i++)
                {
                    //如果没有计算FIRST集
                    if (!first.flag[i])
                    {
                        for (int j = 0; j < grammar.countEachRow[i]; j++)
                        {
                            var it = grammar.grammarTable[i, j].GetEnumerator();
                            //获取产生式的首字符
                            it.MoveNext();
                            it.MoveNext();
                            //如果it没有到边界并且是非终结字符并且并且已经计算FIRST集并且FIRST含有空字
                            while (grammar.grammarTable[i, j].Contains((char)it.Current) && isNonTerminal((char)it.Current) && first.flag[getNonTerminalIndex((char)it.Current)] && hasEmpty(getNonTerminalIndex((char)it.Current)))
                            {
                                first.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                // first.flag[i] = true;
                                calSetUnion(i, getNonTerminalIndex((char)it.Current));
                                it.MoveNext();
                            }
                            //如果it到边界，说明每个非终结符的FIRST集都已经计算出来，并且都含有空字
                            if (!grammar.grammarTable[i, j].Contains((char)it.Current))
                            {
                                //把空字加入
                                first.First[i].Add('@');
                                first.flag[i] = true;
                                continue;
                            }
                            //否则，it没有到边界
                            else
                            {
                                //如果*it为终结符
                                if (isTerminal((char)it.Current))
                                {
                                    first.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                    first.flag[i] = true;
                                    //把终结字符保存到FIRST集
                                    first.First[i].Add((char)it.Current);
                                }
                                //如果是非终结符
                                else if (isNonTerminal((char)it.Current))
                                {
                                    //如果已经计算过FIRST集，则把FIrst集加入
                                    if (first.flag[getNonTerminalIndex((char)it.Current)])
                                    {
                                        first.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                        first.flag[i] = true;
                                        calSetUnion(i, getNonTerminalIndex((char)it.Current));
                                    }
                                    //没有计算过
                                    else
                                    {
                                        first.flag[i] = false;
                                    }
                                }
                                //如果是空字
                                else
                                {
                                    first.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                    first.flag[i] = true;
                                    //把终结字符保存到FIRST集
                                    first.First[i].Add((char)it.Current);
                                }
                            }
                        }
                    }
                    //如果计算FIRST集
                    else
                    {
                        continue;
                    }
                }
            }
        }

        void getFollowSet()
        {
            getPosition();
            calFOLLOWSet();
        }

        void calFOLLOWSet()
        {
            //对于开始符号S，需加入"#"到FOLLOW集
            follow.Follow[0].Add('#');
            while (reloadFOLLOWCalCount() != grammar.count)
            {
                for (int i = 0; i < grammar.count; i++)
                {
                    //如果没有计算FOLLOW集，则计算
                    if (!follow.flag[i])
                    {
                        foreach (Position it in follow.position[i])
                        {
                            int m = it.x;
                            int n = it.y;
                            int index = 1;
                            int a = 0;
                            //使其指向首字符
                            while (index < grammar.grammarTable[m, n].Count)
                            {
                                if (grammar.grammarTable[m, n][index].Equals(grammar.grammarTable[i, 0][0]))
                                {
                                    index++;
                                    break;
                                }
                                index++;
                            }
                            //itp不指向结尾，并且是非终结符并FIRST集含有空字，则继续检测
                            while (index < grammar.grammarTable[m, n].Count && isNonTerminal((char)grammar.grammarTable[m, n][index]) && hasEmpty(getNonTerminalIndex((char)grammar.grammarTable[m, n][index])))
                            {
                                int tempindex = getNonTerminalIndex((char)grammar.grammarTable[m, n][index]);
                                follow.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                //将非终结符去空字的FIRST集加入FOLLOW集
                                calFollowAndFirstUnion(i, tempindex);
                                index++;
                            }
                            //如果itp没有指向end指针，说明该字符为终结字符或非终结字符或空字
                            if (index < grammar.grammarTable[m, n].Count)
                            {
                                if (isTerminal((char)grammar.grammarTable[m, n][index]))
                                {
                                    follow.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                    //将非终结字符加入FOLLOW集
                                    follow.Follow[i].Add((char)grammar.grammarTable[m, n][index]);
                                    //标记已经计算该非终结符的FOLLOW集
                                    follow.flag[i] = true;
                                }
                                else if (isNonTerminal((char)grammar.grammarTable[m, n][index]))
                                {
                                    int tempindex = getNonTerminalIndex((char)grammar.grammarTable[m, n][index]);
                                    follow.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                    //将非终结符去空字的FIRST集加入FOLLOW集
                                    calFollowAndFirstUnion(i, tempindex);
                                    //标记已经计算该非终结符的FOLLOW集
                                    follow.flag[i] = true;
                                }
                                //空字什么也不做
                            }
                            else
                            {
                                //itp枚举完成
                                if (!follow.flag[m])
                                {
                                    //如果没有计算则标记false
                                    follow.flag[i] = false;
                                }
                                else
                                {
                                    follow.nonTerminal[i] = (char)grammar.grammarTable[i, 0][0];
                                    calFollowAndFollowUnion(i, m);
                                    //标记已经计算该非终结符的FOLLOW集
                                    follow.flag[i] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        //构建预测分析表
        void bulidAnalyseTable()
        {
            bool flag = false;
            //遍历每个非终结符
            for (int i = 0; i < grammar.count; i++)
            {
                //遍历每个非终结字符的产生式
                for (int j = 0; j < grammar.countEachRow[i]; j++)
                {
                    flag = false;
                    SortedSet<char> firstSet = buildFirstForOne(i, j);
                    foreach (var it in firstSet)
                    {
                        //如果FIRST集存在空字，记上标记
                        if (isEmpty(it))
                        {
                            flag = true;
                        }
                        //否则将相应的产生式加入预测分析表
                        else
                        {
                            //将文法字符转为字符串
                            string str = charToString(i, j);
                            analyseTable[i, getTerminalIndex(it)] = str;
                        }
                    }
                    //产生式的FIRST集中含有空字
                    if (flag)
                    {
                        //获取i为索引的非终结字符的FOLLOW集
                        foreach (var it in follow.Follow[i])
                        {
                            if (isTerminal(it))
                            {
                                analyseTable[i, getTerminalIndex(it)] = (string)"@";
                            }
                        }
                    }
                    //产生式的FIRST集中不含有空字
                    else
                    {
                        //获取i为索引的非终结字符的FOLLOW集
                        foreach (var it in follow.Follow[i])
                        {
                            analyseTable[i, getTerminalIndex(it)] = (string)"synch";
                        }
                    }
                }
            }
        }

        //构建单个产生式的First集,i,j为相应产生式的索引
        SortedSet<char> buildFirstForOne(int i, int j)
        {
            //定义集合
            SortedSet<char> temp = new SortedSet<char>();
            int itindex = 1;

            while (itindex < grammar.grammarTable[i, j].Count)
            {
                //如果没有出界，并且是非终结字符，并且FIRST集含有空字
                while (itindex < grammar.grammarTable[i, j].Count && isNonTerminal((char)grammar.grammarTable[i, j][itindex]) && hasEmpty(getNonTerminalIndex((char)grammar.grammarTable[i, j][itindex])))
                {
                    int tempindex = getNonTerminalIndex((char)grammar.grammarTable[i, j][itindex]);
                    foreach (var itp in first.First[tempindex])
                    {
                        //如果不是空字则添加temp集合
                        if (!isEmpty(itp))
                        {
                            temp.Add(itp);
                        }
                    }
                    itindex++;
                }
                //没有出界
                if (itindex < grammar.grammarTable[i, j].Count)
                {
                    //如果是终结字符或空字，则把终结字符填到FIRST集
                    if (isTerminal((char)grammar.grammarTable[i, j][itindex]) || isEmpty((char)grammar.grammarTable[i, j][itindex]))
                    {
                        temp.Add((char)grammar.grammarTable[i, j][itindex]);
                        return temp;
                    }
                    //否则为非终结符
                    else
                    {
                        int index = getNonTerminalIndex((char)grammar.grammarTable[i, j][itindex]);
                        foreach (var itpt in first.First[index])
                        {
                            temp.Add(itpt);
                        }
                        return temp;
                    }
                }
                //如果出界，则退出
                else
                {
                    //说明都是非终结字符，且都含有空字
                    temp.Add('@');
                    return temp;
                }
                itindex++;
            }
            return temp;
        }

        //获取终结符在Grammar.terminal[]中的索引
        int getTerminalIndex(char var)
        {
            for (int i = 0; i < grammar.terNum; i++)
            {
                if (grammar.terminalChar[i].Equals(var))
                {
                    return i;
                }
            }
            //不存在返回-1
            return -1;
        }


        //将FIRST集去空加入FOLLOW集，i代表FOLLOW,i代表FIRST集
        void calFollowAndFirstUnion(int i, int j)
        {
            foreach (var it in first.First[j])
                //如果有空字，则去空字

                if (hasEmpty(j))
                {
                    if (!isEmpty(it))
                    {
                        follow.Follow[i].Add(it);
                    }
                }
                else
                {
                    follow.Follow[i].Add(it);
                }
        }

        //将grammarTable中的字符连接为string
        string charToString(int i, int j)
        {
            StringBuilder sb = new StringBuilder();
            var it = grammar.grammarTable[i, j].GetEnumerator();
            it.MoveNext();
            while (it.MoveNext())
            {
                sb.Append(it.Current);
            }
            return sb.ToString();
        }

        //计算两个FOLLOW集的并集
        void calFollowAndFollowUnion(int i, int j)
        {
            if (i == j)
            {
                return;
            }
            foreach (var it in follow.Follow[j])
            {
                follow.Follow[i].Add(it);
            }
        }


        //更新FOLLOW集的calCount
        int reloadFOLLOWCalCount()
        {
            int count = 0;
            for (int i = 0; i < grammar.count; i++)
            {
                if (follow.flag[i])
                {
                    count++;
                }
            }
            follow.calCount = count;
            return count;
        }

        void getPosition()
        {
            for (int i = 0; i < grammar.count; i++)
            {
                for (int j = 0; j < grammar.count; j++)
                {
                    for (int k = 0; k < grammar.countEachRow[j]; k++)
                    {
                        var itp = grammar.grammarTable[j, k].GetEnumerator();
                        itp.MoveNext();
                        while (itp.MoveNext())
                        {
                            if (grammar.grammarTable[i, 0][0].Equals(itp.Current))
                            {
                                //记下其位置
                                follow.position[i].Add(new Position(j, k));
                            }
                        }
                    }
                }
            }
        }

        //计算两个集合的并集，即set(i) = set(i) ∪ set(j)，其中set(j)中去除空字
        void calSetUnion(int i, int j)
        {
            //如果有空字，则去空字
            if (hasEmpty(j))
            {
                foreach (var ch in first.First[j])
                {
                    if (!isEmpty(ch))
                    {
                        first.First[i].Add(ch);
                    }
                }
            }
            else
            {
                foreach (var ch in first.First[j])
                {
                    first.First[i].Add(ch);
                }
            }
        }
        //检测第i个FIRST集是否有空字
        bool hasEmpty(int i)
        {
            foreach (var ch in first.First[i])
            {
                if ((int)ch == 64)
                {
                    return true;
                }
            }
            return false;
        }

        //获取其非终结字符所在的索引
        int getNonTerminalIndex(char var)
        {
            int index = 0;
            //获取其终结字符所在的索引
            for (index = 0; index < grammar.count; index++)
            {
                if (var.Equals(grammar.grammarTable[index, 0][0]))
                {
                    break;
                }
            }
            return index;
        }

        //更新calCount
        int reloadCalCount()
        {
            int count = 0;
            for (int i = 0; i < grammar.count; i++)
            {
                if (first.flag[i])
                {
                    count++;
                }
            }
            first.calCount = count;
            return count;
        }

        //检测一个字符是否为终结字符
        bool isTerminal(char var)
        {
            if ((!isNonTerminal(var)) && (!isEmpty(var)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //检测一个字符是否为非终结字符
        bool isNonTerminal(char var)
        {
            if ((var > 64) && (var < 91))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //检测一个字符是否为空字
        bool isEmpty(char var)
        {
            if ((int)var == 64)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //将字符列表转换为string
        string veToString(ArrayList vec)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var it in vec)
            {
                sb.Append(it);
            }
            return sb.ToString();
        }

        //将字符数组有选择的转化为字符串
        string toString(char[] buf, int start, int end)
        {
            char[] temp = new char[buf.Length + 1];
            int index = 0;
            for (; start <= end; start++)
            {
                temp[index] = buf[start];
                index++;
            }
            return new string(temp).TrimEnd('\0');
        }

    }
}

