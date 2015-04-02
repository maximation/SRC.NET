﻿/*
** Copyright (C) 2011, 2015 Mario Guggenberger <mg@protyposis.net>
**
** This program is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 2 of the License, or
** (at your option) any later version.
**
** This program is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** You should have received a copy of the GNU General Public License
** along with this program; if not, write to the Free Software
** Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSampleRate {
    internal class InteropWrapper {

        public delegate IntPtr d_src_new(ConverterType converter_type, int channels, out int error);
        public delegate IntPtr d_src_delete(IntPtr state);
        public delegate int d_src_process(IntPtr state, ref SRC_DATA data);
        public delegate int d_src_set_ratio(IntPtr state, double new_ratio);
        public delegate int d_src_reset(IntPtr state);
        public delegate int d_src_is_valid_ratio(double ratio);
        public delegate int d_src_error(IntPtr state);
        public delegate string d_src_strerror(int error);

        public static d_src_new src_new;
        public static d_src_delete src_delete;
        public static d_src_process src_process;
        public static d_src_set_ratio src_set_ratio;
        public static d_src_reset src_reset;
        public static d_src_is_valid_ratio src_is_valid_ratio;
        public static d_src_error src_error;
        public static d_src_strerror src_strerror;

        static InteropWrapper() {
            if (Environment.Is64BitProcess) {
                src_new = Interop64.src_new;
                src_delete = Interop64.src_delete;
                src_process = Interop64.src_process;
                src_set_ratio = Interop64.src_set_ratio;
                src_reset = Interop64.src_reset;
                src_is_valid_ratio = Interop64.src_is_valid_ratio;
                src_error = Interop64.src_error;
                src_strerror = Interop64.src_strerror;
            }
            else {
                src_new = Interop32.src_new;
                src_delete = Interop32.src_delete;
                src_process = Interop32.src_process;
                src_set_ratio = Interop32.src_set_ratio;
                src_reset = Interop32.src_reset;
                src_is_valid_ratio = Interop32.src_is_valid_ratio;
                src_error = Interop32.src_error;
                src_strerror = Interop32.src_strerror;
            }
        }
    }
}
