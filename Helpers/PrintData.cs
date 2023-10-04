using System;
using System.Collections.Generic;
using System.Linq;
using ZapClient.Data;
using ZapSurgical.Data;

namespace ZapReport.Helpers
{
    public class PrintData
    {
        private readonly ZapClient.ZapClient _client;
        private readonly Patient _patient;
        private readonly Plan _plan;
        private byte[] _pictureOfPatient;
        private SystemData _planSystemData;
        private PlanSummary _planSummary;
        private PlanData _planData;
        private VOIData _planVOIData;
        private DoseVolumeData _planDVData;
        private DoseVolumeGrid _planDoseVolumeGrid;
        private BeamData _planBeamData;
        private DeliveryData _deliveryData;
        private int _deliveredFraction;
        private DicomSeries _primarySeries;
        private List<DicomSeries> _secondarySeries;
        private User _physician;
        private User _physicist;
        private DateTime _datePhysician;
        private DateTime _datePhysicist;

        public PrintData(ZapClient.ZapClient client, Patient patient, Plan plan)
        {
            _client = client;
            _patient = patient;
            _plan = plan;
        }

        public Patient Patient { get => _patient; }
        
        public Plan Plan { get => _plan; }
        
        public User Physician { get => _physician; set { _physician = value; } }
        
        public User Physicist { get => _physicist; set { _physicist = value; } }
        
        public DateTime DatePhysician { get => _datePhysician; set { _datePhysician = value; } }
        
        public DateTime DatePhysicist { get => _datePhysicist; set { _datePhysicist = value; } }

        public byte[] PictureOfPatient
        {
            get
            {
                if (_pictureOfPatient == null)
                    _pictureOfPatient = _client.GetPictureForPatient(_patient);
                return _pictureOfPatient;
            }
        }

        public SystemData PlanSystemData
        {
            get
            {
                if (_planSystemData == null)
                    _planSystemData = _client.GetSystemDataForPlan(_plan);
                return _planSystemData;
            }
        }

        public PlanSummary PlanSummary
        {
            get
            {
                if (_planSummary == null)
                    _planSummary = _client.GetPlanSummaryForPlan(_plan);
                return _planSummary;
            }
        }

        public PlanData PlanData
        {
            get
            {
                if (_planData == null)
                {
                    _planData = _client.GetPlanDataForPlan(_plan);

                    var users = _client.GetTPSUsers();

                    var approver = users.Where(u => u.UserID.ToUpper().Equals(_planData.Approver.ToUpper())).FirstOrDefault();

                    _planData.Approver = approver?.Name;
                }

                return _planData;
            }
        }

        public VOIData PlanVOIData
        {
            get
            {
                if (_planVOIData == null)
                    _planVOIData = _client.GetVOIsForPlan(_plan);
                return _planVOIData;
            }
        }

        public DoseVolumeData PlanDVData
        {
            get
            {
                if (_planDVData == null)
                    _planDVData = _client.GetDVDataForPlan(_plan);
                return _planDVData;
            }
        }

        public DoseVolumeGrid PlanDoseVolumeGrid
        {
            get
            {
                if (_planDoseVolumeGrid == null)
                    _planDoseVolumeGrid = _client.GetDoseVolumeGridForPlan(_plan);
                return _planDoseVolumeGrid;
            }
        }

        public BeamData PlanBeamData
        {
            get
            {
                if (_planBeamData == null)
                    _planBeamData = _client.GetBeamsForPlan(_plan);
                return _planBeamData;
            }
        }

        public DeliveryData DeliveryData
        {
            get
            {
                if (_deliveryData == null)
                    _deliveryData = _client.GetDeliveryDataForPlan(_plan);
                return _deliveryData;
            }
        }

        public int DeliveredFraction { get => _deliveredFraction; set { _deliveredFraction = value; } }

        public DicomSeries PrimarySeries
        {
            get
            {
                if (_primarySeries == null)
                    _primarySeries = _client.GetDicomSeriesForUuid(_patient, _plan.PrimaryCTSeriesID);
                return _primarySeries;

            }
        }

        public List<DicomSeries> SecondarySeries
        {
            get
            {
                if (_secondarySeries == null)
                {
                    _secondarySeries = new List<DicomSeries>();

                    foreach (var uuid in _plan.SecondaryCTSeriesUuidList)
                    {
                        _secondarySeries.Add(_client.GetDicomSeriesForUuid(_patient, uuid));
                    }
                }
                return _secondarySeries;
            }
        }
    }
}
