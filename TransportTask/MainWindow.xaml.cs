using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransportTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NorthwestCornerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (supply, demand, costs) = GetInputData();
                (supply, demand, costs) = BalanceProblem(supply, demand, costs);

                var plan = NorthwestCornerMethod(supply, demand, costs);
                double totalCost = CalculateTotalCost(plan, costs);
                DisplayResult(plan, totalCost);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void MinElementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var (supply, demand, costs) = GetInputData();
                (supply, demand, costs) = BalanceProblem(supply, demand, costs);

                var plan = MinElementMethod(supply, demand, costs);
                double totalCost = CalculateTotalCost(plan, costs);
                DisplayResult(plan, totalCost);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private (int[], int[], int[,]) GetInputData()
        {
            var supply = SupplyInput.Text.Split(',').Select(int.Parse).ToArray();
            var demand = DemandInput.Text.Split(',').Select(int.Parse).ToArray();
            var costRows = CostsInput.Text.Split(';');
            int m = supply.Length;
            int n = demand.Length;

            int[,] costs = new int[m, n];

            for (int i = 0; i < m; i++)
            {
                var costValues = costRows[i].Split(',').Select(int.Parse).ToArray();
                for (int j = 0; j < n; j++)
                {
                    costs[i, j] = costValues[j];
                }
            }

            return (supply, demand, costs);
        }

        private (int[], int[], int[,]) BalanceProblem(int[] supply, int[] demand, int[,] costs)
        {
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            if (totalSupply > totalDemand)
            {
                // Добавляем искусственного потребителя
                Array.Resize(ref demand, demand.Length + 1);
                demand[demand.Length - 1] = totalSupply - totalDemand;

                int m = supply.Length;
                int[,] newCosts = new int[m, demand.Length];
                Array.Copy(costs, newCosts, costs.Length);

                for (int i = 0; i < m; i++)
                {
                    newCosts[i, demand.Length - 1] = 0; // Стоимость для искусственного потребителя
                }

                costs = newCosts;
            }
            else if (totalDemand > totalSupply)
            {
                // Добавляем искусственного поставщика
                Array.Resize(ref supply, supply.Length + 1);
                supply[supply.Length - 1] = totalDemand - totalSupply;

                int n = demand.Length;
                int[,] newCosts = new int[supply.Length, n];
                Array.Copy(costs, newCosts, costs.Length);

                for (int j = 0; j < n; j++)
                {
                    newCosts[supply.Length - 1, j] = 0; // Стоимость для искусственного поставщика
                }

                costs = newCosts;
            }

            return (supply, demand, costs);
        }
        private int[,] NorthwestCornerMethod(int[] supply, int[] demand, int[,] costs)
        {
            int m = supply.Length;
            int n = demand.Length;
            int[,] plan = new int[m, n];

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int allocation = Math.Min(supply[i], demand[j]);
                plan[i, j] = allocation;
                supply[i] -= allocation;
                demand[j] -= allocation;

                if (supply[i] == 0) i++; // Переход к следующему источнику
                if (demand[j] == 0) j++; // Переход к следующему пункту назначения
            }

            return plan;
        }

        private int[,] MinElementMethod(int[] supply, int[] demand, int[,] costs)
        {
            int m = supply.Length;
            int n = demand.Length;
            int[,] plan = new int[m, n];

            while (true)
            {
                // Находим минимальный элемент в матрице затрат
                int minCost = int.MaxValue;
                int minRow = -1;
                int minCol = -1;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (supply[i] > 0 && demand[j] > 0 && costs[i, j] < minCost)
                        {
                            minCost = costs[i, j];
                            minRow = i;
                            minCol = j;
                        }
                    }
                }

                if (minRow == -1 || minCol == -1) break; // Если нет доступных элементов

                // Определяем количество товаров для распределения
                int allocation = Math.Min(supply[minRow], demand[minCol]);
                plan[minRow, minCol] = allocation;
                supply[minRow] -= allocation;
                demand[minCol] -= allocation;
            }

            return plan;
        }

        private double CalculateTotalCost(int[,] plan, int[,] costs)
        {
            double totalCost = 0;
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    totalCost += plan[i, j] * costs[i, j];
                }
            }

            return totalCost;
        }

        private void DisplayResult(int[,] plan, double totalCost)
        {
            string result = "Транспортный план:\n";
            int m = plan.GetLength(0);
            int n = plan.GetLength(1);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result += $"{plan[i, j]} ";
                }
                result += "\n";
            }

            result += $"Общая стоимость: {totalCost}\n";

            MessageBox.Show(result);
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            SupplyInput.Text = string.Empty;
            DemandInput.Text = string.Empty;
            CostsInput.Text = string.Empty;
        }

        private void Button_Fill_Click(object sender, RoutedEventArgs e)
        {
            SupplyInput.Text = "200,350,300";
            DemandInput.Text = "270,130,190,150,110";
            CostsInput.Text = "24,50,55,27,16;50,47,23,17,21;35,59,55,27,41";
        }
    }
}

