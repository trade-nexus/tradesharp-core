CREATE DATABASE  IF NOT EXISTS `tradehub` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `tradehub`;
-- MySQL dump 10.13  Distrib 5.6.17, for Win32 (x86)
--
-- Host: localhost    Database: tradehub
-- ------------------------------------------------------
-- Server version	5.6.22-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `fill`
--

DROP TABLE IF EXISTS `fill`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `fill` (
  `ExecutionId` varchar(50) NOT NULL,
  `ExecutionSize` int(11) NOT NULL,
  `ExecutionPrice` decimal(10,0) NOT NULL,
  `ExecutionDateTime` datetime DEFAULT NULL,
  `ExecutionSide` varchar(50) DEFAULT NULL,
  `ExecutionType` varchar(40) DEFAULT NULL,
  `LeavesQuantity` int(11) DEFAULT NULL,
  `CummalativeQuantity` int(11) DEFAULT NULL,
  `Currency` varchar(50) DEFAULT NULL,
  `AverageExecutionPrice` decimal(10,0) DEFAULT NULL,
  `ExecutionAccount` varchar(50) DEFAULT NULL,
  `ExecutionExchange` varchar(50) DEFAULT NULL,
  `OrderId` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ExecutionId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `orders` (
  `OrderId` varchar(50) NOT NULL,
  `BrokerOrderID` varchar(30) DEFAULT NULL,
  `OrderSide` varchar(10) DEFAULT NULL,
  `OrderDateTime` datetime NOT NULL,
  `OrderSize` int(11) NOT NULL,
  `OrderCurrency` varchar(20) DEFAULT NULL,
  `OrderTif` varchar(20) DEFAULT NULL,
  `OrderExecutionProvider` varchar(30) NOT NULL,
  `OrderStatus` varchar(30) DEFAULT NULL,
  `Exchange` varchar(30) DEFAULT NULL,
  `TriggerPrice` decimal(10,0) DEFAULT NULL,
  `Slippage` decimal(10,0) DEFAULT NULL,
  `Remarks` varchar(50) DEFAULT NULL,
  `LimitPrice` decimal(10,5) DEFAULT NULL,
  `Security` varchar(20) DEFAULT NULL,
  `discriminator` varchar(12) DEFAULT NULL,
  `Symbol` varchar(45) DEFAULT NULL,
  `StrategyId` int(11) DEFAULT NULL,
  PRIMARY KEY (`OrderId`),
  KEY `FK_limitorders` (`Security`),
  CONSTRAINT `FK_limitorders` FOREIGN KEY (`Security`) REFERENCES `security` (`Isin`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `security`
--

DROP TABLE IF EXISTS `security`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `security` (
  `Isin` varchar(20) NOT NULL,
  `Symbol` varchar(20) NOT NULL,
  `SecurityType` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`Isin`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `strategy`
--

DROP TABLE IF EXISTS `strategy`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `strategy` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) DEFAULT NULL,
  `StartDateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

USE tradehub;
DROP TABLE IF EXISTS `TRADEs`;
CREATE TABLE `trades` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TradeSide` varchar(50) DEFAULT NULL,
  `TradeSize` varchar(50) DEFAULT NULL,
  `ProfitAndLoss` decimal(10,5) NOT NULL,
  `Symbol` varchar(50) DEFAULT NULL,
  `StartTime` datetime DEFAULT NULL,
  `CompletionTime` datetime DEFAULT NULL,
  `ExecutionProvider` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`)
);

USE tradehub;
DROP TABLE IF EXISTS `tradedetails`;
CREATE TABLE `tradedetails` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TradeId` int(11) NOT NULL,
  `ExecutionId` varchar(50) DEFAULT NULL,
  `ExecutionSize` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
);

--
-- Dumping events for database 'tradehub'
--

--
-- Dumping routines for database 'tradehub'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2014-12-17 12:48:00
