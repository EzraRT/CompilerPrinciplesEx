import java.util.*;
import java.io.*;

public class ex1 {

    public static void main(String[] args) {
        BufferedReader file = null;
        try {
            file = new BufferedReader(new FileReader("source.txt"));
        } catch (FileNotFoundException e) {
            e.printStackTrace();
            System.out.println("File source.txt not found");
            System.exit(-1);
        }
        String line = null;
        try {
            int i = 1;
            while ((line = file.readLine()) != null) {
                System.out.println("Line " + i);
                Analyzer la = new Analyzer(line);
                la.run();
                la.show();
                ++i;
            }
        } catch (IOException e) {
            e.printStackTrace();
            System.exit(-1);
        }
    }
}

class Pair<E, F> {
    private E first;
    private F second;

    public Pair() {
    }

    public Pair(E e, F f) {
        first = e;
        second = f;
    }

    public E getFirst() {
        return first;
    }

    public void setFirst(E first) {
        this.first = first;
    }

    public F getSecond() {
        return second;
    }

    public void setSecond(F second) {
        this.second = second;
    }
}

class Split {
    String name; // 名称
    Pair<Integer, String> result; // 返回结果
    String type; // 类型
    Pair<Integer, Integer> location; // 位置

    // 根据参数设置单词信息
    Split(String s, int i, String t, int row, int col) {
        name = s;
        result = new Pair<>(i, s);
        type = t;
        location = new Pair<>(row, col);
    }

    @Override
    public String toString() {
        if (result.getFirst() == 7) {
            return name + "\t\t" + type + "\t\t" + type + "\t\t(" + location.getFirst() + "," + location.getSecond() + ")";
        }
        return name + "\t\t(" + result.getFirst() + "," + result.getSecond() + ")\t\t" + type + "\t\t(" + location.getFirst()
                + "," + location.getSecond() + ")";
    }
}

class Analyzer {
    final static char[] delimiter = {';', ',', '(', ')', '[', ']'}; // 分界符
    final static char[] arithmeticOperator = {'+', '-', '*', '/'}; // 算术运算符
    final static String[] keyword = {"do", "end", "for", "if", "printf", "then", "while"}; // 关键字
    final static String[] relationalOperator = {"<", "<=", "=", ">", ">=", "<>"}; // 关系运算符
    String input; // 输入串
    Vector<String> variable_table; // 自定义变量表
    Vector<Split> save; // 分析结果
    // 是数字

    Boolean isDigital(char ch) {
        return ch >= '0' && ch <= '9';
    }

    // 是字母
    Boolean isLetter(char ch) {
        return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z';
    }

    // 是关键字
    Boolean isKeyword(String s) {
        for (String value : keyword) {
            if (s.equals(value)) {
                return true;
            }
        }
        return false;
    }

    // 是分界符
    Boolean isDelimiter(char c) {
        for (char value : delimiter) {
            if (c == value) {
                return true;
            }
        }
        return false;
    }

    // 是算术运算符
    Boolean isArithmetic(char c) {
        for (char value : arithmeticOperator) {
            if (c == value) {
                return true;
            }
        }
        return false;
    }

    public Analyzer(String s) {
        variable_table = new Vector<>();
        save = new Vector<>();
        input = s;
    }

    void run() {
        int row = 1, col = 1;
        int len = input.length();
        int i = 0;
        while (i < len) {
            StringBuilder tmp = new StringBuilder();

            if (input.charAt(i) == '\n') {
                row++;
                col = 1;
                i++;
            } else if (input.charAt(i) == ' ') {
                i++;
            }
            // 字母开头
            else if (isLetter(input.charAt(i))) {
                // 循环读入
                while (isDigital(input.charAt(i)) || isLetter(input.charAt(i))) {
                    tmp.append(input.charAt(i));
                    i++;
                }
                if (isKeyword(tmp.toString())) {
                    Split sp = new Split(tmp.toString(), 1, "关键字", row, col);
                    save.add(sp);
                } else {
                    // 入变量表
                    int flag = 0;
                    for (int j = 0; j < variable_table.size(); j++) {
                        if (tmp.toString().equals(variable_table.elementAt(j))) {
                            flag = 1;
                        }
                    }
                    if (flag != 0) {
                        variable_table.add(tmp.toString());
                    }

                    Split sp = new Split(tmp.toString(), 6, "标识符", row, col);
                    save.add(sp);
                }
                col++;

            }
            // 数字开头
            else if (isDigital(input.charAt(i))) {
                // 循环读入
                while (isDigital(input.charAt(i))) {
                    tmp.append(input.charAt(i));
                    i++;
                }
                if (isDelimiter(input.charAt(i)) || input.charAt(i) == ' ') {
                    Split sp = new Split(tmp.toString(), 5, "常数", row, col);
                    save.add(sp);
                }
                // 数字+字母，非法的标识符
                else {
                    while (isLetter(input.charAt(i)) || isDigital(input.charAt(i))) {
                        tmp.append(input.charAt(i));
                        i++;
                    }
                    Split sp = new Split(tmp.toString(), 7, "Error", row, col);
                    save.add(sp);
                }
                col++;

            } else if (isDelimiter(input.charAt(i))) {
                tmp.append(input.charAt(i));

                Split sp = new Split(tmp.toString(), 2, "分界符", row, col);
                save.add(sp);
                col++;
                i++;
            }
            // 关系运算符
            else if (input.charAt(i) == '>' || input.charAt(i) == '<' || input.charAt(i) == '=') {
                tmp.append(input.charAt(i));
                i++;
                if (input.charAt(i) == '=') {
                    tmp.append(input.charAt(i));
                    i++;
                }

                Split sp = new Split(tmp.toString(), 4, "关系符", row, col);
                save.add(sp);
                col++;
            } else if (isArithmetic(input.charAt(i))) {
                tmp.append(input.charAt(i));
                i++;
                // 算术符+字母或数字，合法
                if (isLetter(input.charAt(i)) || isDigital(input.charAt(i))) {
                    Split sp = new Split(tmp.toString(), 3, "算数符", row, col);
                    save.add(sp);
                } else {
                    while (input.charAt(i) != ' ' && !isDelimiter(input.charAt(i))) {
                        tmp.append(input.charAt(i));
                        i++;
                    }

                    Split sp = new Split(tmp.toString(), 7, "Error", row, col);
                    save.add(sp);
                }
                col++;

            } else {
                while (input.charAt(i) != ' ' && !isDelimiter(input.charAt(i))) {
                    tmp.append(input.charAt(i));
                    i++;
                }

                Split sp = new Split(tmp.toString(), 7, "Error", row, col);
                save.add(sp);
                col++;
            }
        }
    }

    void show() {
        for (int i = 0; i < save.size(); i++) {
            System.out.println(save.elementAt(i));
        }
    }

}
