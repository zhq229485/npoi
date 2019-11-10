﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

namespace NPOI.XSSF.UserModel
{
    using System;
    using System.Collections.Generic;
    using NPOI.OpenXmlFormats.Spreadsheet;
    using NPOI.SS.UserModel;
    using NPOI.XSSF;
    using NUnit.Framework;


    [TestFixture]
    public class TestXSSFTable
    {

        [Test]
        public void Bug56274()
        {
            // read sample file
            XSSFWorkbook inputWorkbook = XSSFTestDataSamples.OpenSampleWorkbook("56274.xlsx");

            // read the original sheet header order
            XSSFRow row = inputWorkbook.GetSheetAt(0).GetRow(0) as XSSFRow;
            List<String> headers = new List<String>();
            foreach (ICell cell in row)
            {
                headers.Add(cell.StringCellValue);
            }

            // no SXSSF class
            // save the worksheet as-is using SXSSF
            //File outputFile = File.CreateTempFile("poi-56274", ".xlsx");
            //SXSSFWorkbook outputWorkbook = new NPOI.XSSF.streaming.SXSSFWorkbook(inputWorkbook);
            //outputWorkbook.Write(new FileOutputStream(outputFile));

            // re-read the saved file and make sure headers in the xml are in the original order
            //inputWorkbook = new NPOI.XSSF.UserModel.XSSFWorkbook(new FileStream(outputFile));
            inputWorkbook = XSSFTestDataSamples.WriteOutAndReadBack(inputWorkbook) as XSSFWorkbook;
            CT_Table ctTable = (inputWorkbook.GetSheetAt(0) as XSSFSheet).GetTables()[0].GetCTTable();
            List<CT_TableColumn> ctTableColumnList = ctTable.tableColumns.tableColumn;

            Assert.AreEqual(headers.Count, ctTableColumnList.Count,
                    "number of headers in xml table should match number of header cells in worksheet");
            for (int i = 0; i < headers.Count; i++)
            {
                Assert.AreEqual(headers[i], ctTableColumnList[i].name,
                    "header name in xml table should match number of header cells in worksheet");
            }
            //Assert.IsTrue(outputFile.Delete());
        }
        [Test]
        public void TestCTTableStyleInfo()
        {
            XSSFWorkbook outputWorkbook = new XSSFWorkbook();
            XSSFSheet sheet = outputWorkbook.CreateSheet() as XSSFSheet;

            //Create
            XSSFTable outputTable = sheet.CreateTable();
            outputTable.DisplayName = ("Test");
            CT_Table outputCTTable = outputTable.GetCTTable();

            //Style configurations
            CT_TableStyleInfo outputStyleInfo = outputCTTable.AddNewTableStyleInfo();
            outputStyleInfo.name = ("TableStyleLight1");
            outputStyleInfo.showColumnStripes = (false);
            outputStyleInfo.showRowStripes = (true);

            XSSFWorkbook inputWorkbook = XSSFTestDataSamples.WriteOutAndReadBack(outputWorkbook) as XSSFWorkbook;
            List<XSSFTable> tables = (inputWorkbook.GetSheetAt(0) as XSSFSheet).GetTables();
            Assert.AreEqual(1, tables.Count, "Tables number");

            XSSFTable inputTable = tables[0];
            Assert.AreEqual(outputTable.DisplayName, inputTable.DisplayName, "Table display name");

            CT_TableStyleInfo inputStyleInfo = inputTable.GetCTTable().tableStyleInfo;
            Assert.AreEqual(outputStyleInfo.name, inputStyleInfo.name, "Style name");
            Assert.AreEqual(outputStyleInfo.showColumnStripes, inputStyleInfo.showColumnStripes, "Show column stripes");
            Assert.AreEqual(outputStyleInfo.showRowStripes, inputStyleInfo.showRowStripes, "Show row stripes");

        }

        [Test]
        public void FindColumnIndex()
        {
            XSSFWorkbook wb = XSSFTestDataSamples.OpenSampleWorkbook("StructuredReferences.xlsx");
            // FIXME: use a worksheet where upper left cell of table is not A1 so that we test
            // that XSSFTable.findColumnIndex returns the column index relative to the first
            // column in the table, not the column number in the sheet
            XSSFTable table = wb.GetTable("\\_Prime.1");
            Assert.IsNotNull(table);
            Assert.AreEqual(0, table.FindColumnIndex("calc='#*'#"));
            Assert.AreEqual(1, table.FindColumnIndex("Name"));
            Assert.AreEqual(2, table.FindColumnIndex("Number"));
            Assert.AreEqual(2, table.FindColumnIndex("NuMbEr"), "case insensitive");
            // findColumnIndex should return -1 if no column header name matches
            Assert.AreEqual(-1, table.FindColumnIndex(null));
            Assert.AreEqual(-1, table.FindColumnIndex(""));
            Assert.AreEqual(-1, table.FindColumnIndex("one"));
            wb.Close();
        }

    }
}
