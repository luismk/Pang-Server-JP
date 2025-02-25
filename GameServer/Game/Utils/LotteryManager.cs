using GameServer.Cmd;
using GameServer.GameType;
using PangyaAPI.SQL.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameServer.Game.Utils
{
    public class LotteryManager
    {
        private class LotteryItem
        {
            public object Value { get; }
            public int BaseProbability { get; }
            public int CurrentProbability { get; set; }
            public bool Active { get; set; }
            public bool CanRepeat { get; }
            public float Penalty { get; }
            public int DrawCount { get; set; }  // Contador de quantas vezes o item foi sorteado
            public int MaxDraws { get; }  // Quantidade máxima de sorteios permitidos

            public LotteryItem(object value, int probability, bool active, bool canRepeat, float penalty, int maxDraws)
            {
                Value = value;
                BaseProbability = probability;
                CurrentProbability = probability;
                Active = active;
                CanRepeat = canRepeat;
                Penalty = penalty;
                MaxDraws = maxDraws;  // Definindo o número máximo de sorteios
                DrawCount = 0;  // Inicializa o contador de sorteios como 0
            }

            public LotteryItem(object value, int probability, bool active, bool canRepeat, float penalty)
            {
                Value = value;
                BaseProbability = probability;
                CurrentProbability = probability;
                Active = active;
                CanRepeat = canRepeat;
                Penalty = penalty;
                MaxDraws = 100;  // Definindo o número máximo de sorteios
                DrawCount = 0;  // Inicializa o contador de sorteios como 0
            }
        }

        private readonly List<LotteryItem> _items = new List<LotteryItem>();

        // Adiciona um item à loteria
        public void AddItem(object value, int probability, bool active, bool canRepeat, float penalty, int maxDraws)
        {
            // Validação para probabilidade ser positiva
            if (probability < 0)
                throw new ArgumentException("A probabilidade deve ser positiva.");

            // A penalidade pode ser qualquer valor positivo, mas podemos aplicar um limite máximo se necessário
            if (penalty <= 0)
                throw new ArgumentException("A penalidade deve ser maior que zero.");


            Debug.WriteLine($"ItemProb: {probability}%");

            _items.Add(new LotteryItem(value, probability, active, canRepeat, penalty, maxDraws));
        }

        // Retorna a soma total das probabilidades dos itens ativos
        public int GetTotalProbability()
        {
            return _items.Where(item => item.Active).Sum(item => item.CurrentProbability);
        }

        // Executa o sorteio aplicando penalidade nos itens raros/lendários
        public object Draw()
        {
            // Filtra itens ativos e que ainda não atingiram o limite de sorteios
            var activeItems = _items.Where(item => item.Active && item.DrawCount < item.MaxDraws).ToList();
            if (!activeItems.Any())
                throw new InvalidOperationException("Nenhum item ativo ou disponível para sorteio.");
                                     trycode:
            // Calcula total de probabilidade
            int totalProbability = GetTotalProbability();
            Random random = new Random();
            int roll = random.Next(1, totalProbability);
            int index = 0;
            // Percorre itens e encontra o premiado
            int cumulative = 0;
            LotteryItem selectedItem = null;
            foreach (var item in activeItems)
            {
                cumulative += item.CurrentProbability;
                if (roll < cumulative)
                {
                    selectedItem = item;
                    index = _items.IndexOf(item);
                    break;
                }
            }

            if (selectedItem == null)
            {
                goto trycode;
            }
            // Aplica a penalidade (aumento ou diminuição da probabilidade)
            selectedItem.CurrentProbability = (int)(selectedItem.CurrentProbability / selectedItem.Penalty);

            // Garante que a probabilidade não caia para zero, e não seja absurdamente alta
            if (selectedItem.CurrentProbability < 1 || selectedItem.CurrentProbability > 10000)
                selectedItem.CurrentProbability = 1; //reinicia

            // Se o item não pode repetir, desativa ele
            if (!selectedItem.CanRepeat)
            {
                selectedItem.Active = false;  // Isso desativa o item após ser sorteado
            }

            // Incrementa o contador de sorteios
            selectedItem.DrawCount++;
            _items[index] = selectedItem;
            return selectedItem.Value;
        }

        // Método para contar itens ativos
        public int GetTotalItem(bool active = true)
        {
            return _items.Where(item => item.Active == active).Count();
        }
    }
}
