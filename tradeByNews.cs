//+------------------------------------------------------------------+
//|                                                  TradeByNews.mq4 |
//|                        Copyright 2016, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2016, MetaQuotes Software Corp."
#property link      "https://www.mql5.com"
#property version   "1.00"
#property strict
//+------------------------------------------------------------------+
//| Expert initialization function                                   |
//+------------------------------------------------------------------+
datetime dateMas[6];
int numberOfNews = ArraySize(dateMas);
int countE= 0;

int OnInit()
  {

//-------------------------------------------------

    dateMas[0]=D'12:33:00';
    dateMas[1]=D'15:00:00';
    dateMas[2]=D'15:30:00';
    dateMas[3]=D'17:00:00';
    dateMas[4]=D'20:00:00';
    dateMas[5]=D'22:30:00';

   /*datetime  GlobalVariableSet(
   string  dateMas,      // имя
   double  dateMas[6]      // устанавлимое значение
   );*/
    Alert("array dates are init");

   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Expert deinitialization function                                 |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
   Alert("game over");

  }
//+------------------------------------------------------------------+
//| Expert tick function                                             |
//+------------------------------------------------------------------+
void OnTick()
  {
//---
    waitForNews();
     return;
   }


//+------------------------------------------------------------------+

void waitForNews(){
  int res = getTimeToEvent();
  Alert("time to news = "+res);
  if (res > 7) {
    Sleep((res - 5)*1000);
  }
  tradeByNews();
}

void tradeByNews(){// +
  RefreshRates();
  bool flag         =false;
  int codeErrorBuy  = -1; //default value
  int codeErrorSell = -1;
  double priseB=Ask+25*_Point; double priseS=Bid-25*_Point;
  int tiketToBuy;
  int tiketToSell;
  double ordB;
  double ordS;

    Alert("setting news orders...");

      tiketToBuy  = OrderSend (Symbol(), OP_BUYSTOP   , 0.01, priseB, 2 , priseB-38*_Point ,priseB+62*_Point );
      codeErrorBuy=GetLastError();
      tiketToSell = OrderSend (Symbol(), OP_SELLSTOP  , 0.01, priseS, 2 , priseS+38*_Point ,priseS-62*_Point );
      codeErrorSell=GetLastError();

   while(!flag){
    if(codeErrorBuy==0 && codeErrorSell==0){ //success

      flag=true;
      Alert("news Orders are set");
      ordB = Ask+25*_Point; double ordBSL = Bid-38*_Point; double ordBTP = Ask+62*_Point;
      ordS = Bid-25*_Point; double ordSSL = Ask+38*_Point; double ordSTP = Bid-62*_Point;

    } else {
    if(codeErrorBuy>0){

      tiketToBuy  = OrderSend (Symbol(), OP_BUYSTOP   , 0.01, priseB, 2 , priseB-38*_Point ,priseB+62*_Point );
      codeErrorBuy=GetLastError();
      Alert(codeErrorBuy);}

    if(codeErrorSell>0 ){
      tiketToSell = OrderSend (Symbol(), OP_SELLSTOP  , 0.01, priseS, 2 , priseS+38*_Point ,priseS-62*_Point );
      codeErrorSell=GetLastError();
      Alert(codeErrorSell);}
    }

  }
  int playTiket = detectPlayOrder(ordB, ordS, tiketToBuy, tiketToSell);
  NewsOrderControl(tiketToBuy, tiketToSell, playTiket);
  return;
}
void NewsOrderControl(int tiketToBuy, int tiketToSell, int playTiket){ // +

  double p=5;
  bool flag = true;

  if (playTiket = 0) {
    //waitForNews();
    return;}

  else if (playTiket=1){  //TODO optimize logic
    OrderSelect(tiketToBuy,SELECT_BY_TICKET,MODE_TRADES);
    playTiket=tiketToBuy;
  }else if(playTiket=2){
    OrderSelect(tiketToSell,SELECT_BY_TICKET,MODE_TRADES);
    playTiket=tiketToSell;
  }
  Alert(GetLastError());

  while(flag){

    double Profit= OrderProfit();

    if(Profit > p){
      Alert("conrol bloc "+" current profit = "+Profit);
      bool Ans=OrderModify(playTiket, Ask ,Bid-38*_Point,Ask+62*_Point,0);  // <----!
      Alert(GetLastError());

      p=p+5;

    }else if(Profit >= 30){
      Alert("final block "+" current profit = "+Profit);
      OrderClose(playTiket,0.1,Ask,3,Red);
      // --- Alena's code
      while  (!OrderClose(playTiket,0.1,Ask,3,Red)) {
        Alert("order can't be closed - error="+GetLastError());
        OrderClose(playTiket,0.1,Ask,3,Red);
      }
      Alert("order closed successfully, profit="+OrderProfit());
      flag = false;
    } // ---
  }
  if (countE = numberOfNews) {
   OnDeinit(0);
  }
 return;
}

int detectPlayOrder(double ordB, double ordS, int tiketToBuy, int tiketToSell){ // +
  int tiket = 0;
  bool orderPlayed=false;
  datetime startTime = TimeCurrent();
  int timeout = 0;
  while(!orderPlayed && timeout <90){
   RefreshRates();

   if(Bid>ordB){
      Alert("play order to Buy");
      tiket = 1;
      orderPlayed = true;
      bool deletedS = OrderDelete(tiketToSell);
      while (!deletedS) {
        deletedS = OrderDelete(tiketToSell);}

    }else if(Ask<ordS){
      Alert("play order to Sell");
      tiket= 2;
      orderPlayed = true;
      bool deletedB = OrderDelete(tiketToBuy);
      while (!deletedB) {
        deletedB = OrderDelete(tiketToBuy);}
    }
    timeout = TimeCurrent() - startTime;
    Alert("timeout = "+timeout);
  }
  if (timeout >= 90) {
      if (tiket = 1) {
         OrderDelete(tiketToSell);}
      else if (tiket = 2) {
         OrderDelete(tiketToBuy);}
  }
  return(tiket);
}

int getTimeToEvent(/*datetime timeEvent*/){ // +
  int res=-25;
  bool flag = true;
  /*double  GlobalVariableGet(
   string  dateMas      // имя
   );*/

  while(res<0 && countE < numberOfNews) {

    datetime timeEvent   = dateMas[countE];
    datetime currentTime = TimeCurrent();

    int sH=TimeHour(currentTime); int sM=TimeMinute(currentTime); int sS=TimeSeconds(currentTime);
    int eH=TimeHour(timeEvent); int eM=TimeMinute(timeEvent); int eS=TimeSeconds(timeEvent);

    int H=eH-sH; int M=eM-sM; int S=eS-sS;
    if(H>0){
      res=H*60*60 + M*60 + S;}
    else if(H<0){
      res=-25;
      countE=countE+1;}
    else if(H==0){

         if(M>0){
             res=M*60+S;}
         else if(M<=0){
             res=-25;
             countE=countE+1;}
          else {res = S;}
    }

  }
  return(res);
   }


//+------------------------------------------------------------------+
//| Tester function                                                  |
//+------------------------------------------------------------------+
/*double OnTester()
  {
//---
   double ret=0.0;
//---

//---
   return(ret);
  }*/
//+------------------------------------------------------------------+
