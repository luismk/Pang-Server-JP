using PangyaAPI.Utilities;
using System;									 

namespace GameServer.GameType
{
		   								   
	public enum PAPEL_SHOP_TYPE : byte
	{
		PST_COMMUN,
		PST_COOKIE,
		PST_RARE
	}

	public enum PAPEL_SHOP_BALL_COLOR : byte
	{
		PSBC_BLUE,
		PSBC_GREEN,
		PSBC_RED
	}

	public class ctx_papel_shop
	{
		public void clear()
		{	  
		}
		public string toString()
		{
			return "NUMERO=" + Convert.ToString(numero) + ", PRICE_NORMAL=" + Convert.ToString(price_normal) + ", PRICE_BIG=" + Convert.ToString(price_big) + ", LIMITTED_PER_DAY=" + Convert.ToString((ushort)limitted_per_day) + ", UPDATE_DATE=" + UtilTime.FormatDate(update_date.ConvertTime());
		}
		public uint numero = new uint(); // Atual Número do Papel Shop
		public ulong price_normal = new ulong(); // Preço do Jogo Normal
		public ulong price_big = new ulong(); // Preço do Jogo Big
		public byte limitted_per_day = 1; // Limitado por dia, tem uma quantidade que pode jogar    // 0 ou 1
		public PangyaTime update_date = new PangyaTime(); // Date de atualização do dia do papel shop
	}

	public class ctx_papel_shop_item
	{
		public void clear()
		{						   
		}
		public uint _typeid = 0;
		public uint probabilidade = 0;
		public int numero = -1; // Número que o papel shop já está
		public PAPEL_SHOP_TYPE tipo;
		public byte active = 1; // Active 0 ou 1
	}

	public class ctx_papel_shop_ball
	{
		public ctx_papel_shop_ball(uint _ul = 0u)
		{
			clear();
		}
		public void clear()
		{									    
		}
		public PAPEL_SHOP_BALL_COLOR color { get; set; }
		public ctx_papel_shop_item ctx_psi = new ctx_papel_shop_item();
		public uint qntd = new uint(); // Qntd do item que foi sorteado
		public object item { get; set; } // stItem, para depois que add no banco de dados, retornar o id, precisa quando envia o pacote de resposta de jogar o papel shop
	}

	public class ctx_papel_shop_coupon
	{
		public void clear()
		{									  
		}
		public uint _typeid = new uint();
 		public byte active = 1; // 0 ou 1
	}	 
}

