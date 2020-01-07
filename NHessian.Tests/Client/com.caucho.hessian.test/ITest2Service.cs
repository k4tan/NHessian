using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.caucho.hessian.test
{
    public interface ITest2Service
    {
        Task methodNull();

        void replyNull();

        bool replyTrue();

        Task<bool> replyFalse();

        Task<int> replyInt_0();

        int replyInt_1();

        int replyInt_47();

        int replyInt_m16();

        int replyInt_0x30();

        int replyInt_0x7ff();

        int replyInt_m17();

        int replyInt_m0x800();

        int replyInt_0x800();

        int replyInt_0x3ffff();

        int replyInt_m0x801();

        int replyInt_m0x40000();

        int replyInt_0x40000();

        int replyInt_0x7fffffff();

        int replyInt_m0x40001();

        int replyInt_m0x80000000();

        Task<long> replyLong_0();

        long replyLong_1();

        long replyLong_15();

        long replyLong_m8();

        long replyLong_0x10();

        long replyLong_0x7ff();

        long replyLong_m9();

        long replyLong_m0x800();

        long replyLong_0x800();

        long replyLong_0x3ffff();

        long replyLong_m0x801();

        long replyLong_m0x40000();

        long replyLong_0x40000();

        long replyLong_0x7fffffff();

        long replyLong_m0x40001();

        long replyLong_m0x80000000();

        long replyLong_0x80000000();

        long replyLong_m0x80000001();

        Task<double> replyDouble_0_0();

        double replyDouble_1_0();

        double replyDouble_2_0();

        double replyDouble_127_0();

        double replyDouble_m128_0();

        double replyDouble_128_0();

        double replyDouble_m129_0();

        double replyDouble_32767_0();

        double replyDouble_m32768_0();

        double replyDouble_0_001();

        double replyDouble_m0_001();

        double replyDouble_65_536();

        double replyDouble_3_14159();

        Task<DateTime> replyDate_0();

        DateTime replyDate_1();

        DateTime replyDate_2();

        Task<string> replyString_0();

        string replyString_null();

        string replyString_1();

        string replyString_31();

        string replyString_32();

        string replyString_1023();

        string replyString_1024();

        string replyString_65536();

        Task<byte[]> replyBinary_0();

        Task<byte[]> replyBinary_null();

        byte[] replyBinary_1();

        byte[] replyBinary_15();

        byte[] replyBinary_16();

        byte[] replyBinary_1023();

        byte[] replyBinary_1024();

        byte[] replyBinary_65536();

        List<object> replyUntypedFixedList_0();

        List<object> replyUntypedFixedList_1();

        List<object> replyUntypedFixedList_7();

        List<object> replyUntypedFixedList_8();

        string[] replyTypedFixedList_0();

        string[] replyTypedFixedList_1();

        string[] replyTypedFixedList_7();

        string[] replyTypedFixedList_8();

        Dictionary<object, object> replyUntypedMap_0();

        Dictionary<object, object> replyUntypedMap_1();

        Dictionary<object, object> replyUntypedMap_2();

        Dictionary<object, object> replyUntypedMap_3();

        Dictionary<object, object> replyTypedMap_0();

        Dictionary<string, int> replyTypedMap_1();

        Dictionary<int, string> replyTypedMap_2();

        Dictionary<List<object>, int> replyTypedMap_3();

        A0 replyObject_0();

        List<object> replyObject_16();

        TestObject replyObject_1();

        List<object> replyObject_2();

        List<TestObject> replyObject_2a();

        List<TestObject> replyObject_2b();

        TestCons replyObject_3();

        Task<bool> argNull(object paramObject);

        bool argTrue(bool paramObject);

        Task<bool> argFalse(bool paramObject);

        Task<bool> argInt_0(int paramObject);

        bool argInt_1(int paramObject);

        bool argInt_47(int paramObject);

        bool argInt_m16(int paramObject);

        bool argInt_0x30(int paramObject);

        bool argInt_0x7ff(int paramObject);

        bool argInt_m17(int paramObject);

        bool argInt_m0x800(int paramObject);

        bool argInt_0x800(int paramObject);

        bool argInt_0x3ffff(int paramObject);

        bool argInt_m0x801(int paramObject);

        bool argInt_m0x40000(int paramObject);

        bool argInt_0x40000(int paramObject);

        bool argInt_0x7fffffff(int paramObject);

        bool argInt_m0x40001(int paramObject);

        bool argInt_m0x80000000(int paramObject);

        Task<bool> argLong_0(long paramObject);

        bool argLong_1(long paramObject);

        bool argLong_15(long paramObject);

        bool argLong_m8(long paramObject);

        bool argLong_0x10(long paramObject);

        bool argLong_0x7ff(long paramObject);

        bool argLong_m9(long paramObject);

        bool argLong_m0x800(long paramObject);

        bool argLong_0x800(long paramObject);

        bool argLong_0x3ffff(long paramObject);

        bool argLong_m0x801(long paramObject);

        bool argLong_m0x40000(long paramObject);

        bool argLong_0x40000(long paramObject);

        bool argLong_0x7fffffff(long paramObject);

        bool argLong_m0x40001(long paramObject);

        bool argLong_m0x80000000(long paramObject);

        bool argLong_0x80000000(long paramObject);

        bool argLong_m0x80000001(long paramObject);

        Task<bool> argDouble_0_0(double paramObject);

        bool argDouble_1_0(double paramObject);

        bool argDouble_2_0(double paramObject);

        bool argDouble_127_0(double paramObject);

        bool argDouble_m128_0(double paramObject);

        bool argDouble_128_0(double paramObject);

        bool argDouble_m129_0(double paramObject);

        bool argDouble_32767_0(double paramObject);

        bool argDouble_m32768_0(double paramObject);

        bool argDouble_0_001(double paramObject);

        bool argDouble_m0_001(double paramObject);

        bool argDouble_65_536(double paramObject);

        bool argDouble_3_14159(double paramObject);

        Task<bool> argDate_0(DateTime paramObject);

        bool argDate_1(DateTime paramObject);

        bool argDate_2(DateTime paramObject);

        bool argString_0(string paramObject);

        bool argString_1(string paramObject);

        bool argString_31(string paramObject);

        bool argString_32(string paramObject);

        bool argString_1023(string paramObject);

        bool argString_1024(string paramObject);

        bool argString_65536(string paramObject);

        bool argBinary_0(byte[] paramObject);

        bool argBinary_1(byte[] paramObject);

        bool argBinary_15(byte[] paramObject);

        bool argBinary_16(byte[] paramObject);

        bool argBinary_1023(byte[] paramObject);

        bool argBinary_1024(byte[] paramObject);

        bool argBinary_65536(byte[] paramObject);

        bool argUntypedFixedList_0(List<object> paramObject);

        bool argUntypedFixedList_1(List<object> paramObject);

        bool argUntypedFixedList_7(List<object> paramObject);

        bool argUntypedFixedList_8(List<object> paramObject);

        bool argTypedFixedList_0(string[] paramObject);

        bool argTypedFixedList_1(string[] paramObject);

        bool argTypedFixedList_7(string[] paramObject);

        bool argTypedFixedList_8(string[] paramObject);

        bool argUntypedMap_0(Dictionary<object, object> paramObject);

        bool argUntypedMap_1(Dictionary<object, object> paramObject);

        bool argUntypedMap_2(Dictionary<object, object> paramObject);

        bool argUntypedMap_3(Dictionary<object, object> paramObject);

        bool argTypedMap_0(Dictionary<object, object> paramObject);

        bool argTypedMap_1(Dictionary<string, int> paramObject);

        bool argTypedMap_2(Dictionary<int, string> paramObject);

        bool argTypedMap_3(Dictionary<List<object>, int> paramObject);

        bool argObject_0(A0 paramObject);

        bool argObject_16(List<object> paramObject);

        bool argObject_1(TestObject paramObject);

        bool argObject_2(List<object> paramObject);

        bool argObject_2a(List<TestObject> paramObject);

        bool argObject_2b(List<TestObject> paramObject);

        bool argObject_3(TestCons paramObject);
    }
}
