/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

10/01/2024	1.0.0.1		TMC, Skyline	Initial version
****************************************************************************
*/

namespace GQI_Get_Bookings_Count_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface;

    [GQIMetaData(Name = "Get Bookings Count")]
    public class DataSource : IGQIDataSource, IGQIInputArguments
    {
        private readonly GQIStringArgument elasticUriArgument = new GQIStringArgument("IP address of Elastic node")
        {
            IsRequired = true,
        };

        private readonly GQIDateTimeArgument startDateArgument = new GQIDateTimeArgument("Start Date")
        {
            IsRequired = false,
            DefaultValue = DateTime.MinValue,
        };

        private readonly GQIDateTimeArgument endDateArgument = new GQIDateTimeArgument("End Date")
        {
            IsRequired = false,
            DefaultValue = DateTime.MaxValue,
        };

        private string elasticUri;
        private DateTime startDate;
        private DateTime endDate;

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[]
            {
                elasticUriArgument,
                startDateArgument,
                endDateArgument,
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            elasticUri = args.GetArgumentValue(elasticUriArgument);
            startDate = args.GetArgumentValue(startDateArgument);
            endDate = args.GetArgumentValue(endDateArgument);
            return new OnArgumentsProcessedOutputArgs();
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Title"),
                new GQIIntColumn("Count"),
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            int bookingsCount = ElasticSearch
                .GetBookings(elasticUri)
                .Count(x =>
                {
                    if (x.CustomData.End < this.startDate)
                    {
                        return false;
                    }

                    if (x.CustomData.Start > this.endDate)
                    {
                        return false;
                    }

                    return true;
                });

            var rows = new List<GQIRow>();
            var cell = new GQICell[]
            {
                new GQICell() {Value = "Number of reservations"},
                new GQICell() { Value = bookingsCount },
            };

            var row = new GQIRow(cell);
            rows.Add(row);

            return new GQIPage(rows.ToArray());
        }
    }
}