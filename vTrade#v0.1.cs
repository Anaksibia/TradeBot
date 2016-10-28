#property copyright "i"
#property link      "https://www.mql5.com"
#property version   "1.00"
#property strict
#property script_show_inputs
//--- input parameters
input datetime Input1;
input datetime Input2;
input datetime Input3;
input datetime Input4;
input datetime Input5;
input datetime Input6;
input datetime Input7;
input datetime Input8;
input datetime Input9;
input datetime Input10;
input datetime Input11;
input datetime Input12;
input datetime Input13;
input datetime Input14;
input datetime Input15;
input datetime Input16;
input datetime Input17;
input datetime Input18;
input datetime Input19;
input datetime Input20;
input datetime Input21;
input datetime Input22;
input datetime Input23;
input datetime Input24;
input datetime Input25;
input datetime Input26;
input datetime Input27;
input datetime Input28;
input datetime Input29;
input datetime Input30;
//----------------------------------------------
input int M;
int pause=15;
int dC=47;
int minP=12;
double B,A;
int tiketToBuy; //ticket
int tiketToSell;
  
double ordB ; double ordS ;
//-------------------------------------------------
  datetime dateMas[30];
//+------------------------------------------------------------------+
//| Script program start function                                    |
//+------------------------------------------------------------------+
  void OnInit()
   {
    dateMas[0]=Input1;dateMas[11]=Input12;dateMas[12]=Input13;dateMas[23]=Input24;dateMas[24]=Input25;
    dateMas[1]=Input2;dateMas[10]=Input11;dateMas[13]=Input14;dateMas[22]=Input23;dateMas[25]=Input26;
    dateMas[2]=Input3;dateMas[9]=Input10;dateMas[14]=Input15;dateMas[21]=Input22;dateMas[26]=Input27;
    dateMas[3]=Input4;dateMas[8]=Input9;dateMas[15]=Input16;dateMas[20]=Input21;dateMas[27]=Input28;
    dateMas[4]=Input5;dateMas[7]=Input8;dateMas[16]=Input17;dateMas[19]=Input20;dateMas[28]=Input29;
    dateMas[5]=Input6;dateMas[6]=Input7;dateMas[17]=Input18;dateMas[18]=Input19;dateMas[29]=Input30;
    
    Alert("array dates is init");
    return;                                     
   }   
  
  void OnStart(){//router
    
    int countE= 0;     
    bool flag = getStrategiToRouter(); //strategy

      while(true){
         if(flag){tradeByVelosity();}
              
         else { tradeByNews();}
      }
     return;
   }

  void OnDeinit(){
    Alert("game over");

   } 
//+------------------------------------------------------------------+
void tradeByNews(){

  int pause=15;//seconds 
  double B,A; 
     
  waitByNews(); //waitForNews
  
  bool flag = setTwoOrderByNews();
  
  if(flag){
    NewsOrderControl(tiketToBuy, tiketToSell); 
  }
  return;                     
}
void tradeByVelosity(){

    bool flagOrder= false;
    bool TRADE = true;

    while(TRADE){
      
      bool stayInThisMethod = getStrategiToRouter(); //   <----+-

      int target = getStrategiByVelosity();   //   <----+
       
      if(target == 1 && flagOrder == false && stayInThisMethod == true){
        int tiket = getBuyOrder(); //   <----+
        flagOrder=true;
        OrderControl(tiket); //   <----+
        flagOrder = false;

      }else if(target == 2 && flagOrder == false && stayInThisMethod == true){
        int tiket = getSellOrder(); //   <----+
        flagOrder = true;
        OrderControl(tiket); //   <----+
        flagOrder = false;

      } 
        //else if(flagOrder = true && stayInThisMethod = true){
        //OrderControl(); <----!
        //flagOrder = false; }
      else if(stayInThisMethod = false){
        TRADE     = false;
        flagOrder = false;
      }
    }
   return;
}
bool getStrategiToRouter(){// +
  bool flag =  false;
  int res = getTimeToEvent();
  
  if(res <= 7){ flag = false;}
  else if(res >=12){ flag = true;}
  return(flag);
}
void waitByNews(){// +

  Alert("prepare bloc");
  bool flag=true;

  while(flag){
    int timeToEvent = getTimeToEvent();
    if(timeToEvent==-25||timeToEvent < pause){continue;}
    else if(timeToEvent > pause){
         Alert("time to news = "+getTimeToEvent());
         Sleep((timeToEvent-pause)*1000);
         flag=false;
        }
  }
  
  RefreshRates(); 
  B=Bid; A=Ask; 
  return;
}
bool setTwoOrderByNews(){// +
  
  bool flag         =false;
  int codeErrorBuy  = -1; //default value
  int codeErrorSell = -1;
  double priseB=Ask+25*_Point; double priseS=Bid-25*_Point;
  int tiketToBuy;
  int tiketToSell;

  while(!flag){
    Alert("bloc buy LATER ORDER");
   
   if(codeErrorBuy==-1 && codeErrorSell==-1){ 
     
      tiketToBuy  = OrderSend (Symbol(), OP_BUYSTOP   , 0.01, priseB, 2 , priseB-38*_Point ,priseB+62*_Point ); 
      codeErrorBuy=GetLastError();
      tiketToSell = OrderSend (Symbol(), OP_SELLSTOP  , 0.01, priseS, 2 , priseS+38*_Point ,priseS-62*_Point );
      codeErrorSell=GetLastError();
     
    }else if(codeErrorBuy==0 && codeErrorSell==0){ //success

      flag=true;
      Alert("news Order is set");
      double ordB = Ask+25*_Point; double ordBSL = Bid-38*_Point; double ordBTP = Ask+62*_Point;
      double ordS = Bid-25*_Point; double ordSSL = Ask+38*_Point; double ordSTP = Bid-62*_Point;       
  
    } else if(codeErrorBuy!=0 && codeErrorBuy!=-1 && codeErrorSell==0){ 
      tiketToBuy  = OrderSend (Symbol(), OP_BUYSTOP   , 0.01, priseB, 2 , priseB-38*_Point ,priseB+62*_Point ); 
      codeErrorBuy=GetLastError();
    
    }else if(codeErrorSell!=0 && codeErrorSell!=-1 && codeErrorBuy==0 ){
      tiketToSell = OrderSend (Symbol(), OP_SELLSTOP  , 0.01, priseS, 2 , priseS+38*_Point ,priseS-62*_Point );
      codeErrorSell=GetLastError();
    } //what if both unsuccess?

  }
  
  return(flag);      
}
void NewsOrderControl(int tiketBuy, int tiketSell){ // +

  double p=5;    
  bool flag = true;
  int playTiket = detectPlayOrder();
  
  if(playTiket=1){  //TODO optimize logic
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
      Alert("final bloc "+" current profit = "+Profit);
      OrderClose(playTiket,0.1,Ask,3,Red);
      // --- Alena's code
      while  (!OrderClose(playTiket,0.1,Ask,3,Red)) {
        Alert("order can't be closed - error="+GetLastError());
        OrderClose(playTiket,0.1,Ask,3,Red);
      }
      Alert("order closed successfully, profit="+OrderProfit());
      flag = false;
    } // ---
  
    Sleep(300);
  }     
 return;
}

int detectPlayOrder(){ // +
  int tiket;
  bool flag=false;
  while(!flag){ //timeout???
   RefreshRates();
        
   if(Bid>ordB){
      Alert("play order to Buy");
      tiket = 1;
      flag = true;     
       OrderDelete(tiketToSell);
      
    }else if(Ask<ordS){
      Alert("play order to Sell");
      tiket= 2;
      flag = true;
      OrderDelete(tiketToBuy);
    }
  }  
  return(tiket);
}
//==================================================================
double getVel_0(){
    double close_array[3];
    double open_array[3];
    
    CopyClose(Symbol(),PERIOD_M1,1,3,close_array); 
    CopyOpen(Symbol(),PERIOD_M1,1,3,open_array); 
      
    double close3 =close_array[0]; 
    double open1=open_array[2];
    
    double resVe=(open1-close3)*100000;
    return(resVe);
}
double getVel_1(){
    double close_array[3];
   
    CopyClose(Symbol(),PERIOD_M1,1,3,close_array); 
     
    double close2 =close_array[1]; 
    double close1=close_array[2];
    
    double resVe=(close1-close2)*100000;
    return(resVe);
}
double getA(){
   double resA=getVel_1()-getVel_0();
   return(resA);
} 
//=======================================================

int getStrategiByVelosity(){ // +
   
  int dC =30;  
  int res= 0;
  double close_array[3];

  double V_1 = getVel_1();
  double A   = getA();
  double Pv  = 20*MathAbs(getVel_1())-dC; // <----!
  double vectorTrend = close_array[2] - close_array[1];
  CopyClose(Symbol(),PERIOD_M5,1,3,close_array); 
 
  if(V_1 > 0  && A >0 && Pv>60 && vectorTrend > 0) { res = 1; } // buy}
  else if( V_1 < 0 && A <0 && Pv>60 && vectorTrend < 0){ res = 2;} // sell}
  return(res); 
}
int getTimeToEvent(/*datetime timeEvent*/){ // +
  int res=-25;
  bool flag = true;
  int countE=0;  
  while(flag){
  
    datetime timeEvent   = dateMas[countE];  
    datetime currentTime = TimeCurrent(); 

    int sH=TimeHour(currentTime); int sM=TimeMinute(currentTime); int sS=TimeSeconds(currentTime);
    int eH=TimeHour(timeEvent); int eM=TimeMinute(timeEvent); int eS=TimeSeconds(timeEvent);
   
    int H=eH-sH; int M=eM-sM; int S=eS-sS;
    if(H>0){
      res=H*60*60;
      flag = false;}
    else if(H<0){
      res=-25;
      countE=countE+1;}
    else if(H==0){
         
         if(M>0){
             res=M*60+S;
             flag = false;}
         else if(M<=0){
             res=-25;
             countE=countE+1;}
    }
  }
  return(res); }
  
 //======================================
 int getBuyOrder(){ // +

  Alert("buy bloc");
  Alert("[A="+getA()+"] "+" [v="+getVel_1()+"]");
  
  int tiketToBuy=OrderSend(Symbol(),OP_BUY,0.1,Ask , 2 , Bid-20*_Point ,Ask+30*_Point );
  Alert(GetLastError());
  
  return(tiketToBuy);
}
int getSellOrder(){// +
  
  Alert("sell bloc");
  Alert("[A="+getA()+"] "+" [v="+getVel_1()+"]");
  
  int tiketToSell=OrderSend(Symbol(),OP_SELL,0.1, Bid , 2 , Ask+20*_Point ,Bid-30*_Point );
  Alert(GetLastError());

  return(tiketToSell);
}
void OrderControl(int tiket){ // +
   
  double p=0.2;    
  bool flag = true;

  OrderSelect(tiket,SELECT_BY_TICKET,MODE_TRADES);
  Alert(GetLastError());
  
  while(flag){

    double Profit= OrderProfit();
               
    if(Profit > p){
      Alert("conrol bloc "+" curent profit = "+Profit);
      bool Ans=OrderModify(tiket, Bid ,Ask-15*_Point, Bid+100*_Point,0);  // <----!
      Alert(GetLastError());
     
      p=p+0.2;
    
    }else if(Profit >= 5){
      Alert("final bloc "+" curent profit = "+Profit);
      OrderClose(tiket,0.1,Ask,3,Red);
      Alert("order close  with error="+GetLastError());  
      flag = false;
    }
  
    Sleep(1000);
  
  }     

 return;
