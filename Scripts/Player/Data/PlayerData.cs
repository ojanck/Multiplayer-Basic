using Multiplayer.Smartfox;
using Multiplayer.Smartfox.Network;
using Newtonsoft.Json;
using UnityEngine;

namespace Local.Player.Data
{
    public class PlayerData : MonoBehaviour
    {
        [SerializeField] private int _id;
        [SerializeField] private string _username;
        [SerializeField] private bool _isMyself;
        public int Id { get => _id; set => _id = value; }
        public string Username { get => _username; set => _username = value; }
        public bool IsMyself { get => _isMyself; set => _isMyself = value; }

        public static readonly float sendingPeriod = 0.1f; 
        private readonly float accuracy = 0.002f;
        private float timeLastSending = 0.0f;
        private bool send = false;
        private TransformHandler lastState;
        private TransformInterpolation interpolator;

        public static PlayerData FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PlayerData>(json);
        }

        public static string ToString(PlayerData json)
        {
            return JsonConvert.SerializeObject(json);
        }

        private void Start()
        {
            lastState = TransformHandler.FromTransform(transform);
            interpolator = GetComponent<TransformInterpolation>();
            interpolator?.StartReceiving();
        }

        private void FixedUpdate()
        {
            if (IsMyself)
            {
                SendTransform();
            }
        }

        public void ReceiveTransform(TransformHandler nTransform)
        {
            if (interpolator != null)
            {
                // interpolating received transform
                interpolator.ReceivedTransform(nTransform);
            }
            else
            {
                //No interpolation - updating transform directly
                transform.position = nTransform.Position;
                // Ignoring x and z rotation angles
                transform.localEulerAngles = nTransform.AngleRotationFPS;
            }
        }

        public void SendTransform()
        {
            if (timeLastSending >= sendingPeriod)
            {
                lastState = TransformHandler.FromTransform(transform);
                GameController.instance.SendTransform(lastState);
                timeLastSending = 0;
                return;
            }
            timeLastSending += Time.deltaTime;
        }
    }
}