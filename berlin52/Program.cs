using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace berlin52
{
    class Program
    {



        static void Main(string[] args)

           //C://Users//mtrzepacz//Desktop//useless//berlin52.txt 20 200000 1
        {

            if (args[0].Equals("h") || args[0].Equals("?"))
            {
                Console.WriteLine("args[0] = sciezka do pliku \n args[1] - wielkosc populacji \n args[2] ilosc iteracji \n args[3] - '0' - ruletka/'1' - turniej");
                Console.ReadKey();
            }
            else
            {
                //inicjalizacja
                var ber = new BerlinTab(args[0]);
                int[,] population = ber.getRandomPopulation(int.Parse(args[1]));
                int[] shortestPath = new int[population.GetLength(1)];
                int shortestValue = int.MaxValue;
                int currMin = 0;
                var lastMin = int.MaxValue;
                //int currMax;
                int shortestIndex;
                int howMuchSameTables = 0;
                int z;
                int[] par1;
                int[] par2;
                int iteracje = int.Parse(args[2]);
                bool czyRuletka = false;
                  if(int.Parse(args[3])== 0)
                {
                    czyRuletka = true;
                }


                //iteracje
                for (z = 0; z < iteracje; z++)
                {
                    var evaluated = ber.Evaluate(population);
                    lastMin = currMin;
                    currMin = evaluated.Min();
                    //currMax = evaluated.Max();


                    if (currMin == lastMin)
                    {

                        if (howMuchSameTables == 200000)
                        {
                            break;
                        }
                        else
                        {
                            howMuchSameTables++;
                        }

                    }
                    else
                    {
                        howMuchSameTables = 0;
                    }



                    //if (currMin == 0)
                    //{
                    //    throw new ArgumentNullException();
                    //}


                    if (currMin < shortestValue)
                    {
                        shortestIndex = evaluated.IndexOf(currMin);
                        for (int i = 0; i < population.GetLength(1); i++)
                        {
                            shortestPath[i] = population[shortestIndex, i];
                        }
                        shortestValue = currMin;

                    }

                    ber.GetPercentageEvaluation(evaluated);
                    List<int[]> newPopulation = new List<int[]>();
                    for (int i = 0; i < population.GetLength(0) / 2; i++)
                    {
                        if (czyRuletka)
                        {
                            par1 = ber.getParentRoulette(population);
                            par2 = ber.getParentRoulette(population);

                        }
                        else
                        {
                            //tournament
                            par1 = ber.getParentTournament(population);
                             par2 = ber.getParentTournament(population);
                        }

                        

                        newPopulation = ber.Cross2PArents(newPopulation, par1, par2);
                    }

                    var temp = newPopulation.ToArray();
                    for (int i = 0; i < population.GetLength(0); i++)
                    {
                        for (int j = 0; j < population.GetLength(1); j++)
                        {

                            population[i, j] = temp[i][j];
                        }
                    }

                }

                //BerlinTab.PrintTable(population);
                Console.WriteLine("======== grande finale==============");
                //var final =  ber.Evaluate(population);
                //for (int i = 0; i < final.Count; i++)
                //{
                //    Console.WriteLine(i+" "+final[i]);
                //}
                Console.WriteLine("SHORTEST VALUE: " + shortestValue + "\nIterations: " + z);
                for (int i = 0; i < shortestPath.Length; i++)
                {
                    Console.Write(shortestPath[i] + " ");
                }
                Console.ReadKey();

            }
        }
    }
    


    class BerlinTab
    {

        public int[,] mainArr;
        private double[] percentageChances;
        private double[] sorted;
        private int len0;
        private int len1;
        private int mutationChance;
        private Random randomInt;
        private Random randomDouble;

        public BerlinTab(string filePath)
        {

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    bool firstLine = true;
                    int count = 0;

                    while (sr.EndOfStream == false)
                    {
                        if (firstLine == true)
                        {
                            int num = int.Parse(sr.ReadLine());
                            mainArr = new int[num, num];
                            firstLine = false;
                        }
                        else
                        {
                            string[] line = sr.ReadLine().Split(null as string[], StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < line.Length; i++)
                            {
                                mainArr[count, i] = int.Parse(line[i]);
                                mainArr[i, count] = int.Parse(line[i]);
                            }

                            count++;

                        }

                    }
                }
                //inicjalizacja randomów
                randomInt = new Random();
                randomDouble = new Random();
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }





        }

        public static void PrintTable(int[,] table)
        {
            for (int i = 0; i < table.GetLength(0); i++)
            {
                for (int j = 0; j < table.GetLength(1); j++)
                {
                    Console.Write(table[i, j].ToString());
                    Console.Write("\t");



                }
                Console.WriteLine("======================================\n"+i+ "======================================");
            }
        }



        public List<int> Evaluate(int[,] population)
        {

          //  Console.WriteLine("\n\n");
            List<int> evaluate = new List<int>();

            for (int i = 0; i < len0; i++)
            {
                var rowSum = 0;
                for(int j=0; j < len1; j++)
                {
                    if (j == (len1- 1))
                    {
                        rowSum = rowSum + mainArr[population[i, j], population[i,0]];
                    }
                    else
                    {
                        rowSum = rowSum + mainArr[population[i,j], population[i,j + 1]];
                    }

                }
    //            Console.WriteLine(rowSum+" "+i);
                evaluate.Add(rowSum);
            }
            return evaluate;
        }


        public void GetPercentageEvaluation(List<int> evaluated)
        {

            ////Wartość najgorszego osobnika-Wartość danego osobnika+1)/(suma wartości wszystkich osobników+1)
            int len = evaluated.Count;
            double max = evaluated.Max();
            double[] fin = new double[len];
            this.mutationChance = 5;

            if (evaluated.Min() == max)
            {
                for (int i = 0; i < len; i++)
                {
                    fin[i] = 1;
                }
                this.percentageChances = fin;
                sorted = new double[this.percentageChances.Count()];
                this.percentageChances.CopyTo(sorted, 0);
                Array.Sort(sorted);
                this.mutationChance = 10;
                return;
            }

      
            double sum = evaluated.Sum(); 

            for (int i = 0; i < len; i++)
            {
                fin[i] = ((max - evaluated[i]) + 1 /(sum + 1));
                //Console.WriteLine(fin[i]);
            }

            //normalization
            var normalized = new double[len];
            max = fin.Max();
            double min = fin.Min();

            for (int i = 0; i < len; i++)
            {
                normalized[i] = (fin[i] - min) / (max - min);
                //Console.WriteLine(fin[i]);
            }

            if (normalized.Max()!= 1)
            {
                normalized = fin;
               // throw new ArgumentNullException();
            }

   
            this.percentageChances = normalized;
            sorted = new double[len0];
            this.percentageChances.CopyTo(sorted, 0);
            Array.Sort(sorted);


        }

        public int[] getParentTournament(int[,] population)
        {

            int[] parent1 = new int[len1];
            int win = -1;
            
            int r1 = randomInt.Next(0,len0);
            int r2 = randomInt.Next(0, len0);
            int r3 = randomInt.Next(0, len0);
            if (percentageChances[r1] == percentageChances[r2] && percentageChances[r2] == percentageChances[r3])
            {
                win = r1;
            }
            else if(percentageChances[r1]>= percentageChances[r2] && percentageChances[r1]>= percentageChances[r3])
            {
                win = r1;

            }
            else if (percentageChances[r2] >= percentageChances[r1] && percentageChances[r2] >= percentageChances[r3])
            {

                win = r2;
            }
            else
            {

                win = r3;

            }


            for (int i = 0; i < len1; i++)
            {
                parent1[i] = population[win, i];
            }

            return parent1;
            
        }


        //public int[,] getParentsFromRoulette(int[,] population, float[] percentageChances)
        public int[] getParentRoulette(int[,] population)
        {
            //double[] unsorted =  new double[percentageChances.Count()];
            // percentageChances.CopyTo(unsorted,0);
            //Array.Sort(percentageChances);

            //wybranie rodzicow
            int[] parent1 = new int[len1];
            double r = randomDouble.NextDouble();
            for (int i = 0; i < len0; i++)
            {
                if (r < sorted[i]) {
                   int index =  Array.IndexOf(percentageChances, sorted[i]);
                    for (int j = 0; j < len1; j++)
                    {
                        parent1[j] = population[index, j];
                    }
                    break;
                }
            }
            return parent1;

        }


        public List<int[]> Cross2PArents(List<int[]> newPopulation, int[] parent1, int[] parent2)
        {
            int ilePozycji = randomInt.Next(1, len1/2);

            int shouldReproduce = randomInt.Next(0, 100);

            int[] child1 = new int[len1];
            int[] child2 = new int[len1];
            for (int i = 0; i < len1; i++)
            {
                child1[i] = -1;
                child2[i] = -1;
            }


            //krzyżowanie
            if (shouldReproduce < 75)
            {

                
                //PMX

                int pos1 = randomInt.Next(0, len1);
                int pos2 = randomInt.Next(0, len1);
                if (pos2 < pos1)
                {
                    var temp = pos1;
                    pos1 = pos2;
                    pos2 = temp;
                }

                int firstIndex;
                int secIndex;
                for (int i = pos1; i < pos2; i++)
                {
                   
                        firstIndex = Array.IndexOf(parent2, parent1[i]);
                    if (parent1[i] != parent2[firstIndex])
                    {
                        child2[firstIndex] = parent2[i];
                        child2[i] = parent1[i];
                    }
                    secIndex = Array.IndexOf(parent1, parent2[i]);
                    if (parent2[i] != parent1[secIndex])
                    {

                        child1[secIndex] = parent1[i];
                        child1[i] = parent2[i];

                    }
                    
  
                }

                for (int j = 0; j < len1; j++)
                {
                    if (child1[j] == -1)
                    {
                        child1[j] = parent2[j];

                    }

                    if (child2[j] == -1)
                    {
                        child2[j] = parent1[j];

                    }

                }

            }
            else
            {

                  child1 = parent1;
                  child2 = parent2;

            }

            child1 = Mutation(child1);
            child2 = Mutation(child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
            return newPopulation;
         

        }

        private int[] Mutation(int[] child)
        {
            int shouldMutate = randomInt.Next(0, 1000);
            //  int len = child.Length;
            int pos1;
            int pos2;


            //mutacja
            if (shouldMutate <= mutationChance)
            {
                //inwersja
                pos1 = randomInt.Next(0, len1);
                pos2 = randomInt.Next(0, len1);
                while (pos1 == pos2)
                {
                    pos2 = randomInt.Next(0, len1);
                }

                if (pos2 < pos1)
                {
                    var temp = pos1;
                    pos1 = pos2;
                    pos2 = temp;
                }

                int j = 0;
                int[] subArray = new int[pos2-pos1];
                for (int i = pos1; i < pos2; i++)
                {
                    subArray[j] = child[i];
                    j++;
                }

                subArray = subArray.Reverse().ToArray();


                j = 0;
                for (int i = pos1; i < pos2; i++)
                {
                    child[i] = subArray[j++];
                }
            


                 
                ////wymiana genów
                //var pos1 = randomInt.Next(0, len);
                //var pos2 = randomInt.Next(0, len);
                ////upewnienie sie, ze pozycje sa rozne
                //while (pos1 == pos2)
                //{
                //    pos2 = randomInt.Next(0, len);
                //}

                //var temp = child[pos1];
                //child[pos1] = child[pos2];
                //child[pos2] = temp;
                
            }
            return child;
        }


        public int[,] getRandomPopulation(int howMuch)
        {
            int main0 = mainArr.GetLength(0);
            int main1 = mainArr.GetLength(1);
            int[,] randomArr = new int[howMuch, main0];

            List<int> usedIndexes = new List<int>();
            int index = 0;
            int x = 0;

            while (howMuch != 0)
            {
                x = randomInt.Next(main0);
                howMuch--;
                usedIndexes = new List<int>();

                for (int i = 0; i < main1; i++)
                {
                    while (usedIndexes.IndexOf(index) > -1)
                    {
                        index = randomInt.Next(main0);
                    }
                    usedIndexes.Add(index);
                    randomArr[howMuch, i] = index;
                }

            }
            this.len0 = randomArr.GetLength(0);
            this.len1 = randomArr.GetLength(1);
            return randomArr;
        }
    }

}

